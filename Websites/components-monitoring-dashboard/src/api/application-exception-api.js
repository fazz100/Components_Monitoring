import { getConfig } from '../config/config';
import { authFetch, handleApiResponse } from '../helpers/auth-token-helper';

// the bearer token is added by authFetch
async function getHeaders() {
    return {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
    };
}


export async function getExceptions( appName = null) {
    const config = await getConfig();

    const baseUrl = `${config.API_BASE_URL}/api/exceptions/all/`;

    const params = new URLSearchParams();
    if (appName) params.append('appName', appName);

    const queryString = params.toString();
    const finalUrl = queryString ? `${baseUrl}?${queryString}` : baseUrl;

    const response = await authFetch(finalUrl, {
        method: 'GET',
        headers: await getHeaders()
    });
    return handleApiResponse(response);
}

export async function createException(model) {
    const config = await getConfig();
    const response = await authFetch(`${config.API_BASE_URL}/api/exceptions/create`, {
        method: 'POST',
        headers: await getHeaders(),
        body: JSON.stringify(model)
    });
    return handleApiResponse(response);
}

export async function deleteException(id) {
    const config = await getConfig();
    const response = await authFetch(`${config.API_BASE_URL}/api/exceptions/delete/${id}`, {
        method: 'DELETE',
        headers: await getHeaders()
    });
    return handleApiResponse(response);
}