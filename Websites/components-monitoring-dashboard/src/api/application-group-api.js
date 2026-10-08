import { getConfig } from '../config/config';
import { getAuthToken, handleApiResponse } from '../helpers/auth-token-helper';

async function getHeaders() {
    const config = await getConfig();
    const token = getAuthToken();
    return {
        'Authorization': `Bearer ${token}`,
        'X-Api-Token': config.API_AUTH_TOKEN,
        'Content-Type': 'application/json',
        'Accept': 'application/json'
    };
}

export async function getApplicationGroups(searchTerm = null) {
    const config = await getConfig();

    const baseUrl = `${config.API_BASE_URL}/api/application-groups/all`;

    const params = new URLSearchParams();
    if (searchTerm) params.append('searchTerm', searchTerm);

    const queryString = params.toString();
    const finalUrl = queryString ? `${baseUrl}?${queryString}` : baseUrl;

    const response = await fetch(finalUrl, {
        method: 'GET',
        headers: await getHeaders()
    });
    return handleApiResponse(response);
}

export async function createApplicationGroup(model) {
    const config = await getConfig();
    const response = await fetch(`${config.API_BASE_URL}/api/application-groups/create`, {
        method: 'POST',
        headers: await getHeaders(),
        body: JSON.stringify(model)
    });
    return handleApiResponse(response);
}

export async function updateApplicationGroup(model) {
    const config = await getConfig();
    const response = await fetch(`${config.API_BASE_URL}/api/application-groups/update`, {
        method: 'POST',
        headers: await getHeaders(),
        body: JSON.stringify(model)
    });
    return handleApiResponse(response);
}

export async function deleteApplicationGroup(id) {
    const config = await getConfig();
    const response = await fetch(`${config.API_BASE_URL}/api/application-groups/delete/${id}`, {
        method: 'POST',
        headers: await getHeaders()
    });
    return handleApiResponse(response);
}
