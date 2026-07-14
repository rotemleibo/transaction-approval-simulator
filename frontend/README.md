# Frontend - Transaction Approval Simulator

React + TypeScript frontend for simulating transaction approval decisions by selected region and local transaction time.

## Tech

- React 19 + TypeScript + Vite
- TanStack React Query (server state + pagination)
- Axios (HTTP client)
- React Hook Form + Zod
- i18next + react-i18next (English/Hebrew)
- CSS Modules

## Features

- Signup/Login UI integrated with backend JWT endpoints
- Token persistence in localStorage
- Auto logout on API 401 responses
- Region catalog fetch and search
- Transaction simulation form (region + date + time)
- Result card for Approved/Rejected decisions
- Approved transactions carousel with paged fetching (infinite query)
- EN/HE language toggle with persistent selection
- Automatic RTL/LTR switching on language change

## Project Structure

```text
src/
  app/
  features/
    auth/
    transactions/
  i18n/
  layout/
  services/api/
  styles/
  types/
```

## Environment

- VITE_API_BASE_URL: backend base URL
- Default in code: http://localhost:5000
- Recommended for this repository local backend profile: http://localhost:8080

Example .env file:

```env
VITE_API_BASE_URL=http://localhost:8080
```

## Scripts

```bash
npm install
npm run dev
npm run lint
npm run build
npm run preview
```

## Local Run

1. Start backend first (see root README).
2. Start frontend with npm run dev.
3. Open http://localhost:5173.

## Docker

When using root docker-compose.yml:

- frontend is exposed at http://localhost:8081
- VITE_API_BASE_URL is injected at build time as http://localhost:8080
