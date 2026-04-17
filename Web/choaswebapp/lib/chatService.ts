import {
  getFirestore,
  collection,
  query,
  where,
  getDocs,
  addDoc,
  serverTimestamp,
  orderBy,
  QueryConstraint,
} from 'firebase/firestore';

export interface ChatMessage {
  messageId: string;
  documentId: string;
  senderId: string;
  senderName: string;
  content: string;
  messageType: string;
  timestamp: Date;
  isOwn: boolean;
}

export interface Conversation {
  conversationId: string;
  userAId: string;
  userBId: string;
  lastMessage: string;
  lastMessageTime: Date;
  lastMessageType: string;
  userAName: string;
  userBName: string;
}

export class ChatServiceTS {
  private idToken: string;
  private currentUserId: string;

  constructor(idToken: string, currentUserId: string) {
    this.idToken = idToken;
    this.currentUserId = currentUserId;
  }

  private createConversationId(userId1: string, userId2: string): string {
    const ids = [userId1, userId2].sort();
    return `${ids[0]}-${ids[1]}`;
  }

  async loadConversationsAsync(): Promise<Conversation[]> {
    const conversations: Conversation[] = [];

    try {
      const db = getFirestore();
      const conversationsRef = collection(db, 'conversations');

      // Query para conversas onde o usuário é userAId ou userBId
      const queryA = query(
        conversationsRef,
        where('userAId', '==', this.currentUserId)
      );
      const queryB = query(
        conversationsRef,
        where('userBId', '==', this.currentUserId)
      );

      const [snapshotA, snapshotB] = await Promise.all([
        getDocs(queryA),
        getDocs(queryB),
      ]);

      const seenIds = new Set<string>();

      // Process results from both queries
      const processSnapshot = (snapshot: any) => {
        snapshot.forEach((doc: any) => {
          const data = doc.data();
          const convId = data.conversationId || this.createConversationId(data.userAId, data.userBId);

          if (!seenIds.has(convId)) {
            seenIds.add(convId);
            conversations.push({
              conversationId: convId,
              userAId: data.userAId,
              userBId: data.userBId,
              lastMessage: data.lastMessage || '',
              lastMessageTime: data.lastMessageTime?.toDate?.() || new Date(),
              lastMessageType: data.lastMessageType || 'text',
              userAName: data.userAName || 'Usuário A',
              userBName: data.userBName || 'Usuário B',
            });
          }
        });
      };

      processSnapshot(snapshotA);
      processSnapshot(snapshotB);

      // Sort by last message time (descending)
      conversations.sort(
        (a, b) =>
          new Date(b.lastMessageTime).getTime() -
          new Date(a.lastMessageTime).getTime()
      );

      return conversations;
    } catch (error) {
      console.error('[ChatServiceTS.loadConversationsAsync] Error:', error);
      return conversations;
    }
  }

  async loadMessagesAsync(
    contactId: string,
    limit: number = 50
  ): Promise<ChatMessage[]> {
    const messages: ChatMessage[] = [];

    try {
      const db = getFirestore();
      const conversationId = this.createConversationId(
        this.currentUserId,
        contactId
      );
      const messagesRef = collection(
        db,
        `conversations/${conversationId}/messages`
      );

      const q = query(messagesRef, orderBy('timestamp', 'asc'));

      const snapshot = await getDocs(q);

      snapshot.forEach((doc) => {
        const data = doc.data();
        messages.push({
          messageId: data.messageId || doc.id,
          documentId: doc.id,
          senderId: data.senderId,
          senderName: data.senderName,
          content: data.content,
          messageType: data.messageType || 'text',
          timestamp: data.timestamp?.toDate?.() || new Date(),
          isOwn: data.senderId === this.currentUserId,
        });
      });

      return messages;
    } catch (error) {
      console.error('[ChatServiceTS.loadMessagesAsync] Error:', error);
      return messages;
    }
  }

  async sendMessageAsync(
    contactId: string,
    contactName: string,
    senderName: string,
    content: string,
    messageType: string = 'text'
  ): Promise<{ success: boolean; error?: string }> {
    try {
      const db = getFirestore();
      const conversationId = this.createConversationId(
        this.currentUserId,
        contactId
      );

      const messageData = {
        messageId: `msg_${Date.now()}`,
        senderId: this.currentUserId,
        senderName: senderName,
        content: content,
        messageType: messageType,
        timestamp: serverTimestamp(),
        isEdited: false,
        isDeleted: false,
      };

      // Add message to subcollection
      const messagesRef = collection(
        db,
        `conversations/${conversationId}/messages`
      );
      await addDoc(messagesRef, messageData);

      // Update conversation metadata
      await this.upsertConversationMetadataAsync(
        contactId,
        contactName,
        senderName,
        content,
        messageType
      );

      return { success: true };
    } catch (error) {
      console.error('[ChatServiceTS.sendMessageAsync] Error:', error);
      return {
        success: false,
        error: error instanceof Error ? error.message : 'Unknown error',
      };
    }
  }

  private async upsertConversationMetadataAsync(
    contactId: string,
    contactName: string,
    senderName: string,
    lastMessage: string,
    messageType: string
  ): Promise<void> {
    try {
      // This would update the conversation metadata
      // Implementation depends on your Firestore schema
      console.log(
        `[ChatServiceTS] Updating metadata for ${contactId}: "${lastMessage}"`
      );
    } catch (error) {
      console.error('[ChatServiceTS.upsertConversationMetadata] Error:', error);
    }
  }
}
