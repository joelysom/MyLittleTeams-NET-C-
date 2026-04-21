This is a [Next.js](https://nextjs.org) project with Firebase integration.

## Setup

1. Ensure you have Node.js >= 20.9.0. If not, set your PATH to the correct Node version:
   ```powershell
   $env:PATH="C:\Users\JOELYSONALCANTARADAS\Downloads\node-v24.14.1-win-x64;" + $env:PATH
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Configure Firebase:
   - Go to [Firebase Console](https://console.firebase.google.com/)
   - Create a new project or use existing
   - Get your Firebase config from Project Settings > General > Your apps
   - Update `lib/firebase.ts` with your config

4. Run the development server:
   ```bash
   npm run dev
   ```

Open [http://localhost:3000](http://localhost:3000) with your browser to see the result.

## Deploy to Vercel

This project is ready to deploy from the `Web/choaswebapp` folder.

1. Import the project into Vercel with the project root set to `Web/choaswebapp`.
2. Let Vercel use the provided `vercel.json` configuration.
3. Add these environment variables in Vercel Project Settings:
   - `NEXT_PUBLIC_FIREBASE_API_KEY`
   - `NEXT_PUBLIC_FIREBASE_AUTH_DOMAIN`
   - `NEXT_PUBLIC_FIREBASE_PROJECT_ID`
   - `NEXT_PUBLIC_FIREBASE_STORAGE_BUCKET`
4. Keep any secrets out of the client bundle. Only variables prefixed with `NEXT_PUBLIC_` are exposed to the browser.

## Features

- Modern landing page with navbar
- Firebase integration ready
- Tailwind CSS for styling
- TypeScript support

## Next Steps

- Add authentication with Firebase Auth
- Create user dashboard
- Implement Firestore for data storage
- Add more pages (About, Contact, etc.)
