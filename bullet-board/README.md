# Bulletin Board

A bulletin board web app built with Angular 21. Users can browse, search, and filter listings, and authenticated users can post, edit, and delete their own ads.

## Features

- Browse advertisements with search and category filtering
- Post new ads with title, description, category, price, location, and contact info
- Edit and delete your own ads (requires sign-in)
- Location map view powered by Google Maps
- User authentication — register a new account or sign in to an existing one

## Tech stack

- **Angular 21** with standalone components and signals
- **Angular Signals Store** for state management
- **Angular Google Maps** for location display
- **Font Awesome** for icons
- REST API backend expected at `http://localhost:5149` (proxied via `/api`)

## Getting started

### Prerequisites

- Node.js 20+
- A running backend API at `http://localhost:5149`

### Install dependencies

```bash
npm install
```

### Run the dev server

```bash
npm start
```

Open `http://localhost:4200/` in your browser. The app proxies all `/api` requests to the backend at port 5149.

### Build for production

```bash
npm run build
```

Output goes to `dist/`.

### Run tests

```bash
npm test
```
