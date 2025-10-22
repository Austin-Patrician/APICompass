# KeyChecker Web Frontend

Modern React + TypeScript web application for validating AI API keys.

## Features

- ✅ Single key validation with auto provider detection
- ✅ Real-time feedback and detailed results
- ✅ Validation history tracking
- ✅ Light/Dark theme support
- ✅ Responsive design (mobile, tablet, desktop)
- 🔄 Batch validation (coming soon)

## Quick Start

### Install Dependencies

```bash
npm install
```

### Development

```bash
npm run dev
```

The app will be available at `http://localhost:3000`

### Build for Production

```bash
npm run build
```

### Preview Production Build

```bash
npm run preview
```

## Environment Variables

Create a `.env` file in the web directory:

```env
VITE_API_URL=http://localhost:5000
```

## Project Structure

```
src/
├── components/
│   ├── common/         # Reusable UI components
│   ├── layout/         # Layout components (Header, etc.)
│   ├── validation/     # Validation-specific components
│   └── dashboard/      # Dashboard components
├── hooks/              # Custom React hooks
├── pages/              # Page components
├── services/           # API and storage services
├── store/              # Zustand state management
├── types/              # TypeScript type definitions
├── utils/              # Utility functions
├── App.tsx             # Main app component
└── main.tsx            # Entry point
```

## API Integration

The frontend connects to the KeyChecker API at the URL specified in `VITE_API_URL`. 

Make sure your backend is running:
```bash
cd ../src/APICompass.KeyChecker.API
dotnet run
```

## Technology Stack

- **React 18** - UI library
- **TypeScript** - Type safety
- **Vite** - Build tool
- **Tailwind CSS** - Styling
- **React Router** - Navigation
- **React Query** - Server state
- **Zustand** - Client state
- **React Hook Form** - Form management
- **Lucide React** - Icons
- **React Hot Toast** - Notifications

## Features

### Auto Provider Detection
As you type your API key, the app automatically detects which provider it belongs to.

### Validation Options
- **Check Models**: Verify model access
- **Use Cache**: Use cached results for faster validation
- **Verify Org ID**: Verify organization ID (OpenAI only)

### Result Display
- Color-coded status (green=valid, red=invalid)
- Response time metrics
- Tier and rate limit information
- Quota status
- Expandable detailed information
- Copy to clipboard
- Export as JSON

### History
- Automatic saving of validation results
- Export history as JSON
- Delete individual items or clear all
- Search and filter (coming soon)

## Development

### Code Style

The project uses TypeScript strict mode and ESLint. Follow these guidelines:
- Use functional components with hooks
- Type all props and functions
- Keep components small and focused
- Use Tailwind classes for styling
- Follow the existing file structure

### Adding New Components

1. Create component file in appropriate directory
2. Export component as named export
3. Add TypeScript interfaces for props
4. Use existing common components when possible

### API Calls

Use the services in `src/services/api.ts` and create custom hooks in `src/hooks/` for API calls.

## Troubleshooting

### API Connection Issues

Make sure the backend API is running and the `VITE_API_URL` is correct.

### Build Errors

```bash
# Clean and reinstall
rm -rf node_modules package-lock.json
npm install
```

### TypeScript Errors

Check `tsconfig.json` and ensure all dependencies are properly installed.

## License

See main project LICENSE file.
