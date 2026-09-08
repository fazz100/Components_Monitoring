// Helper function to get the current token
export function getAuthToken() {
  return localStorage.getItem('AuthToken');
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

