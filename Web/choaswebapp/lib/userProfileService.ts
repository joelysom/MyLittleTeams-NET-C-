import {
  getFirestore,
  collection,
  query,
  where,
  getDocs,
  doc,
  setDoc,
  getDoc,
  updateDoc,
} from 'firebase/firestore';
import { AvatarComponents, DEFAULT_AVATAR, normalizeAvatar } from './avatarService';

export interface UserProfile {
  userId: string;
  email: string;
  displayName: string;
  avatar: AvatarComponents;
  profilePhoto?: string;
  bio?: string;
  createdAt: Date;
  updatedAt: Date;
}

export class UserProfileService {
  async getUserProfile(userId: string): Promise<UserProfile | null> {
    try {
      const db = getFirestore();
      const userRef = doc(db, 'users', userId);
      const userSnap = await getDoc(userRef);

      if (!userSnap.exists()) {
        return null;
      }

      const data = userSnap.data();
      
      // Normalize avatar from both old (.NET) and new formats
      const avatarData = {
        body: data.avatarBody || data.body,
        hair: data.avatarHair || data.hair,
        hat: data.avatarHat || data.hat,
        accessory: data.avatarAccessory || data.accessory,
        clothing: data.avatarClothing || data.clothing,
      };

      return {
        userId,
        email: data.email || '',
        displayName: data.displayName || '',
        avatar: normalizeAvatar(avatarData),
        profilePhoto: data.profilePhoto,
        bio: data.bio,
        createdAt: data.createdAt?.toDate() || new Date(),
        updatedAt: data.updatedAt?.toDate() || new Date(),
      };
    } catch (error) {
      console.error('Erro ao buscar perfil:', error);
      return null;
    }
  }

  async createUserProfile(
    userId: string,
    email: string,
    displayName: string,
    avatar: AvatarComponents = DEFAULT_AVATAR
  ): Promise<UserProfile> {
    try {
      const db = getFirestore();
      const now = new Date();

      const profile: UserProfile = {
        userId,
        email,
        displayName,
        avatar,
        createdAt: now,
        updatedAt: now,
      };

      const userRef = doc(db, 'users', userId);
      // Save with both .NET field names (avatarBody, etc) for compatibility
      await setDoc(userRef, {
        email,
        displayName,
        avatarBody: avatar.body,
        avatarHair: avatar.hair,
        avatarHat: avatar.hat,
        avatarAccessory: avatar.accessory,
        avatarClothing: avatar.clothing,
        // Also save in new format
        body: avatar.body,
        hair: avatar.hair,
        hat: avatar.hat,
        accessory: avatar.accessory,
        clothing: avatar.clothing,
        createdAt: now,
        updatedAt: now,
      });

      return profile;
    } catch (error) {
      console.error('Erro ao criar perfil:', error);
      throw error;
    }
  }

  async updateUserAvatar(userId: string, avatar: AvatarComponents): Promise<void> {
    try {
      const db = getFirestore();
      const userRef = doc(db, 'users', userId);
      // Update with both formats for .NET compatibility
      await updateDoc(userRef, {
        avatarBody: avatar.body,
        avatarHair: avatar.hair,
        avatarHat: avatar.hat,
        avatarAccessory: avatar.accessory,
        avatarClothing: avatar.clothing,
        body: avatar.body,
        hair: avatar.hair,
        hat: avatar.hat,
        accessory: avatar.accessory,
        clothing: avatar.clothing,
        updatedAt: new Date(),
      });
    } catch (error) {
      console.error('Erro ao atualizar avatar:', error);
      throw error;
    }
  }

  async updateUserProfile(userId: string, updates: Partial<UserProfile>): Promise<void> {
    try {
      const db = getFirestore();
      const userRef = doc(db, 'users', userId);
      await updateDoc(userRef, {
        ...updates,
        updatedAt: new Date(),
      });
    } catch (error) {
      console.error('Erro ao atualizar perfil:', error);
      throw error;
    }
  }

  async getUsersByIds(userIds: string[]): Promise<Map<string, UserProfile>> {
    try {
      const db = getFirestore();
      const profiles = new Map<string, UserProfile>();

      for (const userId of userIds) {
        const profile = await this.getUserProfile(userId);
        if (profile) {
          profiles.set(userId, profile);
        }
      }

      return profiles;
    } catch (error) {
      console.error('Erro ao buscar perfis:', error);
      return new Map();
    }
  }
}

// Singleton instance
let userProfileServiceInstance: UserProfileService | null = null;

export function getUserProfileService(): UserProfileService {
  if (!userProfileServiceInstance) {
    userProfileServiceInstance = new UserProfileService();
  }
  return userProfileServiceInstance;
}
