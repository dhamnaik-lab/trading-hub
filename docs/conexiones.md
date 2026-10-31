[← Back to documentation](README.md)

# Connections

Use **Connections** (Ctrl+4 on Windows, ⌘4 on macOS) to configure market-data providers and trading accounts. The left pane lists connections; the right pane shows the selected connection's settings.

Each entry has a description and status indicator: inactive, last test succeeded or failed, or not yet tested.

## Add and test a connection

1. Select **New connection** below the list and choose a provider.
2. Enter the provider-specific settings.
3. Select **Test connection**. The application saves the configuration before testing and displays the result.
4. Select **Save** after changing any other settings.

The header includes a customizable connection name and an **Enabled / Disabled** switch. A disabled connection does not appear in the instrument selector or offer accounts for trading. **Delete** removes a connection and its associated saved credentials, not recorded market data. Built-in Simulator, Binance, Bybit, and Rithmic connections have restrictions on deleting the last instance of each type.

Many providers also offer **History on chart open** (30 minutes through seven days). Locally stored or downloaded data loads first; missing history is requested from the provider. See [Data](datos.md).

## Where credentials are stored

Passwords, API secrets, access tokens, and the Databento API key use **macOS Keychain** or **Windows Credential Manager**, rather than being stored in plain text in `connections.json`.

After saving a credential, the field shows a stored-secret indicator. Enter a new value to replace a secret; leaving the field blank generally retains the existing value.

The local `connections.json` contains non-secret connection details such as names, usernames, server identifiers, public API key IDs, and settings. Never commit live credentials or real account records to GitHub.

## Connection capabilities

| Provider | Market data | Requirements | Orders |
|---|---|---|---|
| Simulator | Offline simulated market | None | Simulated account |
| Binance Futures / Spot | Public crypto data and history | None for data; key and secret for trading | Testnet or real, if authorized |
| Bybit | USDT linear perpetual data | None for public data | Data only |
| Rithmic | CME futures data, historical ticks, and order routing | Username, password, system/server | Practice or real |
| AMP Futures | Rithmic-based CME data and accounts | AMP-issued Rithmic credentials | Practice or real |
| Tradovate | CME futures data and accounts | Username, password, API CID and secret | Demo or real |
| cTrader | Broker CFD and FX data | Open API application and account authorization | Data only |
| OANDA | Index/commodity CFDs and FX | API token | Data only |
| Alpaca | US equities and ETFs | API key ID and secret | Data only |
| Databento | Historical CME futures tick data | API key, usage-based charges | Historical data only |
| Interactive Brokers | Connection test | TWS / IB Gateway with API enabled | Connection test only |

Any provider supplying market data can be used with Trading Hub's simulated account.

## Simulator

The offline simulator provides simulated order book and trade activity for SIM-BTC, SIM-ETH, SIM-ES, and SIM-NQ. No account or internet connection is required.

## Binance Futures and Binance Spot

Binance public market data requires no account for charts, DOM, time and sales, or available public history. To configure broker trading:

1. Select **Testnet (practice funds)** or **Real account**. Testnet is the default.
2. Enter the API key and API secret. For a real account, create a key with **trading privileges only; disable withdrawal privileges**. The API-key help link opens the relevant Binance site.
3. For real accounts, explicitly enable **Allow REAL orders from Trading Hub**. Otherwise the account remains read-only and unavailable for order entry.
4. Optionally enable per-order confirmation (on by default) and a **Maximum quantity per order**. A value of zero means no application-side maximum.

## Bybit

Provides public USDT linear perpetual-futures market data. Trading through Bybit accounts is not implemented in this version.

## Rithmic

Rithmic uses its official R|Protocol connection, without requiring R|Trader Pro to be open. It starts disabled until configured.

- **Username / Password:** Supplied by your broker or proprietary trading firm.
- **Server:** The app fetches available Rithmic system/region combinations, shows latency, and provides search and refresh. The selected **system** must match the one issued by your broker. Rithmic's Colo 75 endpoint is available only from the CME data center.
- **Custom server:** Allows a manual `wss://…` address and system identifier.
- **Aggregated quotes:** Offered only for compatible server systems; the client can connect without them.
- **Connection timeout:** Defaults to 10,000 ms.
- **Advanced:** Application name (sometimes requires approval) and the product roots available in the instrument picker (ES, NQ, MES, MNQ, CL, etc.). The app resolves the relevant monthly contract.

Rithmic accounts become available after the order session connects. The same username generally cannot maintain competing sessions from R|Trader Pro, ATAS, NinjaTrader, or another client simultaneously.

### Rithmic troubleshooting

- **Permission denied:** Ensure the server system matches the exact broker/prop-firm assignment. Confirm the password, API/R|Protocol entitlement, and any required approved application name.
- **Data works but orders are unavailable:** The real account may still be **read-only**. Check **Allow orders in real accounts** under the connection and the current trading permissions.
- **Repeated disconnections:** Another platform using the same Rithmic login may be taking over the session.

Never enable real orders simply to resolve a data-access issue; verify the account's intended purpose first.

## AMP Futures

AMP supplies a Rithmic username and password. The initial AMP connection uses the demo Rithmic Paper Trading system and is disabled until configured. For real trading select the broker's assigned production system, commonly Rithmic 01, on the appropriate server. Remaining settings follow [Rithmic](#rithmic).

## Tradovate

1. Select **Demo** or **Real account** (demo is the default).
2. Enter the Tradovate username and password.
3. Enter API **CID** and **Secret** from Tradovate's API Access settings.
4. Review product roots for the instrument picker.
5. For real orders, explicitly enable **Allow orders in real Tradovate accounts**. A **Maximum quantity per order** is optional.

Accounts can appear in Chart Trader and Accounts for CME futures charts sourced from Tradovate or Rithmic. Tradovate API market data may require an additional CME market-data entitlement. Trading Hub can use another data connection to display a chart while routing to an authorized Tradovate account. Competing Tradovate sessions can interrupt data access.

## cTrader

Provides CFD/FX market data and tick history through a participating broker's Open API.

1. At `openapi.ctrader.com`, create an Open API app and register the callback address shown by Trading Hub (for example `http://127.0.0.1:53682/ctrader`).
2. Enter the app's **Client ID** and **Secret**.
3. Choose demo or real account and select **Connect with cTrader**. Authorize in your browser within the permitted window. Alternatively supply an access token issued through your app.
4. If the token contains several accounts, supply the desired cTrader login number; zero selects the first.

The app stores and refreshes the token in the OS credential store. For CFDs, quoted bid changes are approximated as trades of volume one. Footprint and delta therefore represent proxies, not exchange prints.

## OANDA

Provides practice or live CFD/FX pricing and historical five-second candles.

- Choose **Practice account** or **Real account**.
- Enter an API token created in OANDA's API Access page.
- Enter the account ID or leave blank to use the first authorized account.

Trading Hub uses this token for **market data only**, not OANDA orders. Synthetic trade volume is assigned to quotes, so order-flow measures are approximate.

## Alpaca

Supplies US stock/ETF data using an Alpaca practice account and an API key ID and secret.

- **IEX:** Free exchange-limited data.
- **SIP:** Consolidated exchange data, subject to subscription/entitlements.

Historical availability and delays depend on the account and entitlement. Trading Hub does not submit Alpaca orders.

## Databento

Provides historical CME futures tick data (for example NQ, ES, MNQ, MES, CL, GC) with aggressor-side fields and long historical coverage. It is **paid by usage**.

- Enter the Databento API key (`db-…`).
- **Test connection** checks the available date range and estimates the cost of one sample NQ day without downloading the data.
- Databento does not supply live data to the chart instrument picker in this integration. Use **Data → CME futures with Databento**. See [Data](datos.md#databento).

Review the price estimate before approving any historical download.

## Interactive Brokers

Open TWS or IB Gateway with the API enabled (Configure → API → Settings). Enter **Host**, **Port**, and **Client ID**. Common ports include 7497 for TWS paper, 7496 for TWS live, and 4002 for Gateway paper. These are defaults, not guarantees.

This integration currently checks connectivity only; it does not offer IBKR market data or live order execution.

## Real and practice accounts

Chart Trader always includes the simulated account. Other accounts depend on provider credentials and the chart instrument.

| Provider | Practice | Real |
|---|---|---|
| Binance | Testnet | Requires **Allow REAL orders from Trading Hub** |
| Rithmic / AMP | Paper/test systems | Requires **Allow orders in real accounts** |
| Tradovate | Demo | Requires **Allow orders in real Tradovate accounts** |

Without this authorization, real accounts stay read-only: balances, positions, and history may remain visible, but submitting orders is blocked. The UI marks real accounts with **REAL** warnings.

### Enable real-money trading safely

1. First test the workflow with a simulated or broker practice account.
2. Switch explicitly to the intended production account/server.
3. Enable the real-order permission only after verifying account details.
4. Configure an appropriate **Maximum quantity per order**.
5. Save and test the connection, then independently check order confirmations and risk controls.

See [Trading safety](operar.md#safety) for confirmations and additional controls.
