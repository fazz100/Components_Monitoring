import { getConfig } from '../config/config';

// Helper function to get the current access token
export function getAuthToken() {
  return localStorage.getItem('AuthToken');
}

export function getRefreshToken() {
  return localStorage.getItem('RefreshToken');
}

// POSTs an x-www-form-urlencoded request to the OAuth 2.0 /token endpoint
// returns { ok, data } where data is the token response or { error, error_description }
export async function requestToken(params) {
  const config = await getConfig();
  // trim the trailing slash: OWIN only matches "/token", not "//token"
  const tokenUrl = `${config.API_BASE_URL.replace(/\/+$/, '')}/token`;

  const response = await fetch(tokenUrl, {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body: new URLSearchParams({ ...params, client_id: config.OAUTH_CLIENT_ID }),
  });

  const data = await response.json().catch(() => ({}));
  return { ok: response.ok, data };
}

export function saveTokens(tokenResponse) {
  localStorage.setItem('AuthToken', tokenResponse.access_token);
  localStorage.setItem('token', tokenResponse.access_token);
  localStorage.setItem('RefreshToken', tokenResponse.refresh_token);
  localStorage.setItem('TokenExpiry', new Date(Date.now() + tokenResponse.expires_in * 1000).toISOString());
}

// Refresh tokens are single-use, so parallel 401s must share ONE refresh request
let refreshPromise = null;

export function refreshAccessToken() {
  if (!refreshPromise) {
    refreshPromise = (async () => {
      const refreshToken = getRefreshToken();
      if (!refreshToken) return false;

      try {
        const { ok, data } = await requestToken({ grant_type: 'refresh_token', refresh_token: refreshToken });
        if (!ok || !data.access_token) return false;

        saveTokens(data);
        return true;
      } catch (error) {
        console.error('Token refresh error:', error);
        return false;
      }
    })().finally(() => { refreshPromise = null; });
  }
  return refreshPromise;
}

// fetch() wrapper that sends the bearer token and silently refreshes it once on a 401
// options.body may be a function so it is re-evaluated on the retry (e.g. after the refresh token rotated)
export async function authFetch(url, options = {}) {
  const send = () => fetch(url, {
    ...options,
    headers: { ...(options.headers || {}), 'Authorization': `Bearer ${getAuthToken()}` },
    body: typeof options.body === 'function' ? options.body() : options.body,
  });

  let response = await send();

  if (response.status === 401 && await refreshAccessToken()) {
    response = await send();
  }

  return response;
}

export async function handleApiResponse(response) {
  const body = await response.json().catch(() => ({}));
  if (!response.ok) {
    const message = body.Message || 'An error occurred.';
    if (response.status === 401) {
      localStorage.clear();
      window.location.href = '/';
    }
    throw new Error(message);
  }
  return body;
}
