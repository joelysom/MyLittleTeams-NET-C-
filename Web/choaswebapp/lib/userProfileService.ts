import {
  getFirestore,
  doc,
  setDoc,
  getDoc,
  updateDoc,
} from 'firebase/firestore';
import { AvatarComponents, DEFAULT_AVATAR, normalizeAvatar } from './avatarService';
import { resolveFirebaseStorageMediaSource } from './chatMedia';

export interface UserProfile {
  userId: string;
  email: string;
  displayName: string;
  avatar: AvatarComponents;
  profilePhoto?: string;
  profilePhotoDataUri?: string;
  profilePhotoStoragePath?: string;
  profilePhotoSource?: string;
  headline?: string;
  course?: string;
  registration?: string;
  academicDepartment?: string;
  academicFocus?: string;
  bio?: string;
  professionalSummary?: string;
  programmingLanguages?: string;
  phoneNumber?: string;
  calendarEntries?: UserCalendarEntry[];
  createdAt: Date;
  updatedAt: Date;
}

export interface UserCalendarEntry {
  entryId: string;
  date: string;
  entryType: string;
  contextLabel: string;
  title: string;
  notes: string;
  createdAt: string;
}

export class UserProfileService {
  private profileCache = new Map<string, Promise<UserProfile | null>>();

  async getUserProfile(userId: string): Promise<UserProfile | null> {
    if (this.profileCache.has(userId)) {
      return this.profileCache.get(userId) ?? null;
    }

    const profilePromise = this.loadUserProfile(userId);
    this.profileCache.set(userId, profilePromise);

    return profilePromise;
  }

  private async loadUserProfile(userId: string): Promise<UserProfile | null> {
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

      const profilePhotoDataUri = data.profilePhotoDataUri || data.profilePhoto || '';
      const profilePhotoStoragePath = data.profilePhotoStoragePath || data.profilePhotoStorage || '';
      const profilePhotoUrl = data.profilePhotoUrl || data.profilePhotoSource || '';
      const profilePhotoSource =
        (typeof profilePhotoDataUri === 'string' && profilePhotoDataUri.trim()) ||
        (typeof profilePhotoUrl === 'string' && profilePhotoUrl.trim()) ||
        (await resolveFirebaseStorageMediaSource(profilePhotoStoragePath)) ||
        '';

      const calendarEntries = Array.isArray(data.calendarEntries)
        ? data.calendarEntries.map((entry: any, index: number) => ({
            entryId: typeof entry?.entryId === 'string' && entry.entryId.trim() ? entry.entryId : `calendar-${index}`,
            date: typeof entry?.date === 'string' && entry.date.trim() ? entry.date : new Date().toISOString(),
            entryType: typeof entry?.entryType === 'string' ? entry.entryType : 'Aviso',
            contextLabel: typeof entry?.contextLabel === 'string' ? entry.contextLabel : 'Professor',
            title: typeof entry?.title === 'string' ? entry.title : '',
            notes: typeof entry?.notes === 'string' ? entry.notes : '',
            createdAt: typeof entry?.createdAt === 'string' && entry.createdAt.trim() ? entry.createdAt : new Date().toISOString(),
          }))
        : [];

      return {
        userId,
        email: data.email || '',
        displayName: data.displayName || '',
        avatar: normalizeAvatar(avatarData),
        profilePhoto: profilePhotoSource,
        profilePhotoDataUri: profilePhotoDataUri || '',
        profilePhotoStoragePath: profilePhotoStoragePath || '',
        profilePhotoSource: profilePhotoSource || '',
        headline: data.headline || '',
        course: data.course || '',
        registration: data.registration || '',
        academicDepartment: data.academicDepartment || '',
        academicFocus: data.academicFocus || '',
        bio: data.bio || '',
        professionalSummary: data.professionalSummary || data.bio || '',
        programmingLanguages: data.programmingLanguages || '',
        phoneNumber: data.phoneNumber || data.phone || '',
        calendarEntries,
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

      this.profileCache.delete(userId);
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

      this.profileCache.delete(userId);
    } catch (error) {
      console.error('Erro ao atualizar perfil:', error);
      throw error;
    }
  }

  async getUsersByIds(userIds: string[]): Promise<Map<string, UserProfile>> {
    try {
      const profiles = new Map<string, UserProfile>();

      const uniqueUserIds = Array.from(new Set(userIds.filter(Boolean)));
      const results = await Promise.all(uniqueUserIds.map((userId) => this.getUserProfile(userId)));

      results.forEach((profile, index) => {
        if (profile) {
          profiles.set(uniqueUserIds[index], profile);
        }
      });

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
