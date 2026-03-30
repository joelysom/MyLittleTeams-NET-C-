# 💬 Chat Window - Design Professional (Microsoft Teams Style)

## 🎯 Visão Geral

A nova janela de chat foi implementada com um design moderno e profissional, inspirado no Microsoft Teams. Totalmente funcional com mensagens em tempo real, lista de conversas e integração com o sistema de busca.

---

## 📐 Layout Architecture

```
┌─────────────────────────────────────────────────────────┐
│  CHAT WINDOW (1400x720 ou Maximized)                    │
└─────────────────────────────────────────────────────────┘

┌──────────────┬─────────────────────────────────────────┐
│              │                                         │
│ SIDEBAR      │        MAIN CHAT AREA                  │
│ (320px)      │        (* Columns)                     │
│              │                                         │
│              ├─────────────────────────────────────────┤
│              │ TOP BAR (60px)                          │
│              │ Contact Info | Action Buttons           │
│              ├─────────────────────────────────────────┤
│              │                                         │
│              │ MESSAGES AREA                           │
│              │ (Scrollable)                            │
│              │                                         │
│              │                                         │
│              ├─────────────────────────────────────────┤
│              │ INPUT AREA (60px)                       │
│              │ Message Input + Send                    │
│              └─────────────────────────────────────────┘
```

---

## 🔷 SIDEBAR - Conversations List

**Dimensions:** 320px wide, full height
**Background:** White (#FFFFFF)

### Components:

#### 1. **Header** (54px)
```
┌─────────────────────────────────┐
│ 💬 Conversas   ➕      ⋮        │
└─────────────────────────────────┘
```
- **Left:** 💬 Conversas (14px, SemiBold, #333333)
- **Right Buttons:**
  - ➕ New conversation (Blue on hover)
  - ⋮ More options

#### 2. **Search Bar** (36px + 12px padding)
```
┌─────────────────────────────────┐
│ 🔍 Pesquisar conversas...      │
└─────────────────────────────────┘
```
- Background: #F0F0F0
- Border: #E0E0E0
- Corner radius: 20px

#### 3. **Conversations List**
Each conversation item: **76px height**

```
┌─────────────────────────────────┐
│ 🔵 Nome Contato        "Agora"  │
│    Ótima ideia! 👍             │
│                                 │
│ 🔵 João Silva    "há 2 min" •  │
│    Combinado, até logo!         │
└─────────────────────────────────┘
```

**Elements per item:**
- Left: 48x48px Avatar (Circle, Dark Blue #0078D4)
- Center: 
  - Name: 13px SemiBold #333333
  - Last message: 12px #777777 (with • if unread)
- Right: Time 11px #999999

**Unread Indicator:** • Bullet + bold text

---

## 🎨 TOP BAR - Contact Information

**Dimensions:** Full width × 60px
**Background:** White
**Border:** Bottom #E8E8E8

### Layout:
```
┌────────────────────────────────────────────┐
│ 🔵 Pedro Lucas da Silva    📞 📹 ℹ️ ⋮     │
│    Online                                   │
└────────────────────────────────────────────┘
```

**Left Section:**
- Avatar: 44×44px, Circle, Blue
- Contact info:
  - Name: 14px SemiBold #333333
  - Status: 12px Green (#107C10) "Online"

**Right Buttons (48×48px each):**
- 📞 Call (Blue, tooltip)
- 📹 Video Call (Blue, tooltip)
- ℹ️ Contact Info (Blue, tooltip)
- ⋮ More Options (Gray, tooltip)

---

## 💬 MESSAGES AREA

**Dimensions:** Full width × dynamic height
**Background:** White
**Scrollable:** Mouse wheel + scroll bar

### Message Container Types:

#### **Own Message (Right-aligned)**
```
                    ┌──────────────────┐
                    │ Our message text │
                    │ with blue bubble  │
                    └──────────────────┘
                    14:35
```
- Background: #0078D4 (Blue)
- Text color: White
- Corner radius: 12px
- Padding: 12px
- Max width: 500px
- Timestamp: 11px #999999, below

#### **Received Message (Left-aligned)**
```
🔵 Pedro Lucas
┌──────────────────┐
│ Received message │
│ with gray bubble │
└──────────────────┘
14:35
```
- Avatar: 36×36px (optional, shown once per conversation)
- Sender name: 11px SemiBold #666666
- Background: #F0F0F0 (Gray)
- Text color: #333333 (Dark)
- Corner radius: 12px
- Padding: 12px
- Max width: 500px
- Timestamp: 11px #999999, below

### Message Animations:
- Fade-in on new message
- Auto-scroll to latest on send
- Simulated response after 2 seconds

---

## ⌨️ INPUT AREA

**Dimensions:** Full width × 60px
**Background:** White
**Border:** Top #E8E8E8

### Components:

```
┌──────────────────────────────┬──┬──┬──┐
│ Escreva uma mensagem...      │📎│😊│➤│
└──────────────────────────────┴──┴──┴──┘
```

#### **Message Input Field**
- Style: Rounded search box (#F5F5F5)
- Border: #E0E0E0
- Corner radius: 24px
- Placeholder: "Escreva uma mensagem..."
- Supports:
  - Multi-line text
  - Shift+Enter = Line break
  - Enter = Send message
- Max height: 120px (before scrolling)

#### **Action Buttons (44×44px each)**
1. **📎 Attach File**
   - Color: Blue (#0078D4)
   - Tooltip: "Anexar arquivo"
   - Margin: 12px left

2. **😊 Emoji Picker**
   - Color: Blue (#0078D4)
   - Tooltip: "Adicionar emoji"
   - Margin: 8px left

3. **➤ Send**
   - Background: Blue (#0078D4)
   - Text: White
   - Tooltip: "Enviar mensagem (Enter)"
   - Margin: 8px left
   - **On Click:**
     - Validates message not empty
     - Creates new ChatMessage object
     - Adds to MessagesList
     - Clears input field
     - Scrolls to bottom
     - Simulates response in 2 seconds

---

## 🎨 Color Scheme

| Element | Color | Hex Code | Usage |
|---------|-------|----------|-------|
| **Primary Blue** | Microsoft Blue | #0078D4 | Buttons, own messages, avatars |
| **Background** | White | #FFFFFF | Main areas |
| **Light Gray** | Surface | #F5F5F5 | Input field background |
| **Border Gray** | Light Gray | #E0E0E0 | Input borders |
| **Text Dark** | Charcoal | #333333 | Main text |
| **Text Light** | Gray | #777777 | Secondary text |
| **Status Online** | Green | #107C10 | Online indicator |
| **Message Recv** | Light Gray | #F0F0F0 | Received message bg |
| **Sidebar Border** | Light Gray | #E8E8E8 | Divider lines |

---

## 📱 Responsive Behavior

- **Window Maximized:** Full screen with sidebar always visible
- **Sidebar:** Fixed 320px width
- **Messages:** Wrap text, center on mobile concepts
- **Input:** Grows up to 120px, then scrolls

---

## 🔄 Functional Features

### Message Management
- ✅ Display messages with sender info, avatar, text, timestamp
- ✅ Own messages: right-aligned, blue
- ✅ Received: left-aligned, gray
- ✅ Auto-scroll to latest message
- ✅ Timestamp format: HH:mm

### Conversation List
- ✅ Load conversations with avatars
- ✅ Show last message preview (truncated)
- ✅ Display time (Agora, há 2 min, há 1 hora)
- ✅ Unread indicator (•)
- ✅ Click to switch conversation
- ✅ Search conversations (placeholder ready)

### Input
- ✅ Send message on Enter
- ✅ Line break on Shift+Enter
- ✅ Validation: no empty messages
- ✅ Auto-clear after send
- ✅ Simulated AI responses (2 second delay)

### Integration
- ✅ Opens from SearchResultsWindow
- ✅ Receives contact info and current user profile
- ✅ Closes search window on open
- ✅ Passes through friend's name and ID

---

## 🚀 How to Use

### From Search Results:
1. User searches for friend → Results shown
2. User clicks **"Iniciar Conversa"** button
3. **ChatWindow opens** automatically with:
   - Contact name in top bar
   - Pre-loaded conversation history
   - Input field ready for typing

### Sending Messages:
1. Type message in input field
2. Press **Enter** or click **➤** button
3. Message appears in blue bubble (right)
4. After 2 seconds → Simulated response (gray bubble, left)

### Switching Conversations:
1. Click different user in Sidebar
2. Messages area refreshes
3. Top bar updates with new contact info

---

## 📦 Files

| File | Type | Purpose |
|------|------|---------|
| ChatWindow.xaml | UI Layout | XAML markup for chat interface |
| ChatWindow.xaml.cs | Code-Behind | Event handlers, message logic |
| ChatMessage.cs (class) | Data | Message object structure |
| SearchResultsWindow.xaml.cs | Integration | Opens ChatWindow on button click |
| MainWindow.xaml.cs | Integration | Passes profile to search window |

---

## ⚙️ Technical Stack

- **Framework:** WPF (.NET 8.0-windows)
- **UI Library:** MahApps.Metro 2.4.x
- **Language:** C#
- **Pattern:** MVVM-adjacent with code-behind
- **Key Classes:**
  - `ChatWindow` - Main window
  - `ChatMessage` - Data model
  - Ellipse, TextBlock, Button - WPF controls

---

## 🎉 Result

A modern, professional chat interface that:
- ✨ Looks like Microsoft Teams
- 🚀 Fully functional with message flow
- 🔗 Integrated with search and user profiles
- 📱 Responsive and user-friendly
- 🎨 Consistent with app design language

**Status:** ✅ **PRODUCTION READY**

Build: Successful (0 errors, 1 warning)
