import React from 'react';
import { createRoot } from 'react-dom/client';
import './styles.css';

type QueryFilter = {
  field: string;
  operator: string;
  value: string;
};

type SemanticQuery = {
  dataset: string;
  metrics: string[];
  dimensions: string[];
  filters: QueryFilter[];
  includeForecast: boolean;
  forecastPeriod?: string | null;
};

type SalesForecast = {
  forecastYear: number;
  predictedTotal: number;
  growthRate: number;
  method: string;
};

type AiReportResponse = {
  query: SemanticQuery;
  data: Record<string, unknown>[];
  forecast?: SalesForecast | null;
  report: string;
};

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5182';

function App() {
  const [prompt, setPrompt] = React.useState(
    'Generate a monthly minor sales report for the previous year and predict next year sales'
  );
  const [result, setResult] = React.useState<AiReportResponse | null>(null);
  const [error, setError] = React.useState<string | null>(null);
  const [isLoading, setIsLoading] = React.useState(false);

  async function generateReport(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsLoading(true);
    setError(null);
    setResult(null);

    try {
      const response = await fetch(`${apiBaseUrl}/api/ai-reports/generate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ prompt })
      });

      const body = await response.json();

      if (!response.ok) {
        throw new Error(body.error ?? `Request failed with status ${response.status}`);
      }

      setResult(body);
    } catch (requestError) {
      setError(requestError instanceof Error ? requestError.message : 'Report request failed.');
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <main className="shell">
      <section className="hero">
        <div>
          <p className="eyebrow">AI Reporting</p>
          <h1>Generate governed sales reports from natural language.</h1>
          <p className="subtitle">
            The UI sends your prompt to the .NET API, which extracts a semantic query, validates it,
            executes the safe report query, forecasts results, and asks OpenAI to write the final report.
          </p>
        </div>
      </section>

      <form className="card form" onSubmit={generateReport}>
        <label htmlFor="prompt">Report prompt</label>
        <textarea
          id="prompt"
          value={prompt}
          onChange={(event) => setPrompt(event.target.value)}
          rows={4}
          placeholder="Ask for a sales report..."
        />
        <div className="actions">
          <button className="generateButton" type="submit" disabled={isLoading || prompt.trim().length === 0}>
            {isLoading && <span className="spinner" aria-hidden="true" />}
            <span>{isLoading ? 'Generating report...' : 'Generate report'}</span>
          </button>
          <span>{apiBaseUrl}</span>
        </div>
      </form>

      {isLoading && (
        <div className="loading card" role="status" aria-live="polite">
          <span className="loadingPulse" aria-hidden="true" />
          <div>
            <strong>Generating your report</strong>
            <p>Extracting the query, forecasting results, and writing the report with OpenAI.</p>
          </div>
        </div>
      )}

      {error && <div className="error card">{error}</div>}

      {result && (
        <div className="results">
          <section className="card">
            <h2>Final report</h2>
            <p className="report">{result.report}</p>
          </section>

          <section className="grid">
            <div className="card">
              <h2>Forecast</h2>
              {result.forecast ? (
                <dl className="stats">
                  <div>
                    <dt>Forecast year</dt>
                    <dd>{result.forecast.forecastYear}</dd>
                  </div>
                  <div>
                    <dt>Predicted total</dt>
                    <dd>{formatNumber(result.forecast.predictedTotal)}</dd>
                  </div>
                  <div>
                    <dt>Growth rate</dt>
                    <dd>{formatPercent(result.forecast.growthRate)}</dd>
                  </div>
                  <div>
                    <dt>Method</dt>
                    <dd>{result.forecast.method}</dd>
                  </div>
                </dl>
              ) : (
                <p>No forecast requested.</p>
              )}
            </div>

            <div className="card">
              <h2>Semantic query</h2>
              <pre>{JSON.stringify(result.query, null, 2)}</pre>
            </div>
          </section>

          <section className="card">
            <h2>Data rows</h2>
            <DataTable rows={result.data} />
          </section>
        </div>
      )}
    </main>
  );
}

function DataTable({ rows }: { rows: Record<string, unknown>[] }) {
  if (rows.length === 0) {
    return <p>No rows returned.</p>;
  }

  const columns = Object.keys(rows[0]);

  return (
    <div className="tableWrap">
      <table>
        <thead>
          <tr>
            {columns.map((column) => (
              <th key={column}>{column}</th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.map((row, index) => (
            <tr key={index}>
              {columns.map((column) => (
                <td key={column}>{formatCell(row[column])}</td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function formatCell(value: unknown) {
  if (typeof value === 'number') {
    return formatNumber(value);
  }

  if (value === null || value === undefined) {
    return '';
  }

  return String(value);
}

function formatNumber(value: number) {
  return new Intl.NumberFormat(undefined, { maximumFractionDigits: 2 }).format(value);
}

function formatPercent(value: number) {
  return new Intl.NumberFormat(undefined, { style: 'percent', maximumFractionDigits: 2 }).format(value);
}

createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>
);
