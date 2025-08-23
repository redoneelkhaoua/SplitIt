# SplitIt Frontend

Minimal React + Vite + TypeScript client scaffold to interact with the Tailoring API.

## Stack
- React 18
- Vite 5
- TypeScript 5
- React Query
- Axios
- React Hook Form

## Dev
```bash
npm install
npm run dev
```
App served at http://localhost:3000 (proxying /api to http://localhost:5000).

## Auth Flow
1. User logs in (admin/admin123 or staff/staff123) -> JWT stored in memory context.
2. Authorization header automatically applied in fetching hooks (extend later).

## Next
- Replace customers fetch stub with real work orders endpoint.
- Persist token (localStorage) with logout & expiry handling.
- Add pagination & filters UI.
- Error + loading states styling.
- Component tests (Vitest / React Testing Library) optional.
