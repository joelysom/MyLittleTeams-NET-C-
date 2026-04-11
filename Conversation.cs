using System;
using System.Collections.Generic;

namespace MeuApp
{
    public class Conversation
    {
        public string ConversationId { get; set; } = "";
        public string ContactId { get; set; } = "";
        public string ContactName { get; set; } = "";
        public string ContactProfilePhotoDataUri { get; set; } = "";
        public string ContactAvatarBody { get; set; } = "";
        public string ContactAvatarHair { get; set; } = "";
        public string ContactAvatarHat { get; set; } = "";
        public string ContactAvatarAccessory { get; set; } = "";
        public string ContactAvatarClothing { get; set; } = "";
        public string LastMessage { get; set; } = "";
        public DateTime LastMessageTime { get; set; }
        public string LastSenderId { get; set; } = "";
        public DateTime? LastReadAt { get; set; }
        public bool HasUnread { get; set; }
        public bool IsFavorite { get; set; }
        public bool CanRemoveFromFavorites { get; set; } = true;
        public int CustomSortOrder { get; set; }
        public bool IsSelfConversation { get; set; }
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

        public string FormattedTime
        {
            get
            {
                var diff = DateTime.Now - LastMessageTime;
                if (diff.TotalMinutes < 1) return "Agora";
                if (diff.TotalMinutes < 60) return $"há {(int)diff.TotalMinutes}m";
                if (diff.TotalHours < 24) return $"há {(int)diff.TotalHours}h";
                return LastMessageTime.ToString("dd/MM");
            }
        }
    }
}
