# AI Reporting Web

Small React UI for the AI reporting API.

## Run

Start the API:

```powershell
dotnet run --project ..\AiReporting.Api --launch-profile http
```

Start the UI:

```powershell
npm install
npm run dev
```

Open:

```text
http://localhost:5173
```

The UI calls the API at `http://localhost:5182` by default. To point it elsewhere, create a `.env.local` file:

```text
VITE_API_BASE_URL=http://localhost:5182
```
