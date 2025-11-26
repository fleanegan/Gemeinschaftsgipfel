import {useAuthStore} from '@/store/auth';
import {authService} from './api';

const authStore = useAuthStore();

export async function login(credentials: { username: string; password: string }) {
    try {
        const token = await authService.login(credentials);
        authStore.login(token, credentials.username);
        return {token};
    } catch (error) {
        throw error;
    }
}