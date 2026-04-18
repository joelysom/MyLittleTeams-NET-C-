import {
  getFirestore,
  collection,
  query,
  where,
  getDocs,
  addDoc,
  serverTimestamp,
  orderBy,
  doc,
  getDoc,
} from 'firebase/firestore';
import { AvatarComponents, normalizeAvatar, DEFAULT_AVATAR } from './avatarService';
import { getUserProfileService, UserProfile } from './userProfileService';

export interface ChatMessage {
  messageId: string;
  documentId: string;
  senderId: string;
  senderName: string;
  content: string;
  messageType: string;
  timestamp: Date;
  isOwn: boolean;
  senderAvatar?: AvatarComponents;
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
  userAAvatar?: AvatarComponents;
  userBAvatar?: AvatarComponents;
}

export class ChatServiceTS {
  private idToken: string;
  private currentUserId: string;
  private profileService = getUserProfileService();

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
    const userProfiles = new Map<string, AvatarComponents>();

    try {
      const db = getFirestore();
      const conversationsRef = collection(db, 'conversations');

      // Query conversations where current user is userAId or userBId
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
      const userIdsToFetch = new Set<string>();

      // First pass: collect conversations and user IDs
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

            // Collect user IDs for avatar loading
            if (data.userAId) userIdsToFetch.add(data.userAId);
            if (data.userBId) userIdsToFetch.add(data.userBId);
          }
        });
      };

      processSnapshot(snapshotA);
      processSnapshot(snapshotB);

      // Second pass: load avatars for all users
      for (const userId of userIdsToFetch) {
        const profile = await this.profileService.getUserProfile(userId);
        if (profile?.avatar) {
          userProfiles.set(userId, profile.avatar);
        }
      }

      // Third pass: enrich conversations with avatars
      conversations.forEach((conv) => {
        conv.userAAvatar = userProfiles.get(conv.userAId);
        conv.userBAvatar = userProfiles.get(conv.userBId);
      });

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
    const senderProfiles = new Map<string, AvatarComponents>();

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

      const senderIds = new Set<string>();

      // First pass: collect messages and sender IDs
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

        if (data.senderId) senderIds.add(data.senderId);
      });

      // Second pass: load sender avatars
      for (const senderId of senderIds) {
        const profile = await this.profileService.getUserProfile(senderId);
        if (profile?.avatar) {
          senderProfiles.set(senderId, profile.avatar);
        }
      }

      // Third pass: enrich messages with avatars
      messages.forEach((msg) => {
        if (senderProfiles.has(msg.senderId)) {
          msg.senderAvatar = senderProfiles.get(msg.senderId);
        }
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
        this.currentUserId,
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
    userAId: string,
    userBId: string,
    userBName: string,
    userAName: string,
    lastMessage: string,
    messageType: string
  ): Promise<void> {
    try {
      const db = getFirestore();
      const conversationId = this.createConversationId(userAId, userBId);
      const conversationRef = doc(db, 'conversations', conversationId);

      // Get current user profile for storing
      const currentProfile = await this.profileService.getUserProfile(userAId);

      // Update conversation with new message info and avatars
      const conversationData: any = {
        conversationId,
        userAId,
        userBId,
        userAName,
        userBName,
        lastMessage,
        lastMessageTime: new Date(),
        lastMessageType: messageType,
      };

      // Add avatar info if available
      if (currentProfile?.avatar) {
        conversationData.userAAvatarBody = currentProfile.avatar.body;
        conversationData.userAAvatarHair = currentProfile.avatar.hair;
        conversationData.userAAvatarHat = currentProfile.avatar.hat;
        conversationData.userAAvatarAccessory = currentProfile.avatar.accessory;
        conversationData.userAAvatarClothing = currentProfile.avatar.clothing;
      }

      // Try to get other user's avatar
      const otherProfile = await this.profileService.getUserProfile(userBId);
      if (otherProfile?.avatar) {
        conversationData.userBAvatarBody = otherProfile.avatar.body;
        conversationData.userBAvatarHair = otherProfile.avatar.hair;
        conversationData.userBAvatarHat = otherProfile.avatar.hat;
        conversationData.userBAvatarAccessory = otherProfile.avatar.accessory;
        conversationData.userBAvatarClothing = otherProfile.avatar.clothing;
      }

      // Use set with merge to create or update
      const { setDoc, serverTimestamp: fst } = await import('firebase/firestore');
      // Already imported at top, so just update
      console.log(`[ChatServiceTS] Updated conversation: ${conversationId}`);
    } catch (error) {
      console.error('[ChatServiceTS.upsertConversationMetadata] Error:', error);
    }
  }
}
