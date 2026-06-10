import { selectStatus, selectAuthError, selectUser, resetErrorAction } from '@/app-redux/auth/authSlice';
import { LoginRequest } from '@/app-redux/auth/authTypes';
import { useAppDispatch, useAppSelector } from '@/hooks/reduxHooks';
import React, { useState, useEffect } from 'react';
import { loginAction } from '@/app-redux/auth/authActions';

const Login = () => {
    const dispatch = useAppDispatch();
    const status = useAppSelector(selectStatus);
    const error = useAppSelector(selectAuthError);
    const user = useAppSelector(selectUser);

    const [form, setForm] = useState({ email: '', password: '' });

    useEffect(() => {
        return () => {
            dispatch(resetErrorAction());
        };
    }, [dispatch]);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        setForm((prev) => ({ ...prev, [e.target.name]: e.target.value }));
    };

    const getLoginPayload = (): LoginRequest => ({
        ...form,
        ipAddress: '127.0.0.1', // optionally get actual IP from service
        clientInformation: navigator.userAgent,
        currentBrowserTimeZone: Intl.DateTimeFormat().resolvedOptions().timeZone,
    });

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        const payload = getLoginPayload();
        dispatch(loginAction(payload));
    };

    return (
        <div style={{ maxWidth: '400px', margin: 'auto', padding: '1rem' }}>
            <h2>Login</h2>
            <form onSubmit={handleSubmit}>
                <div>
                    <label>Email</label>
                    <input type="email" name="email" value={form.email} onChange={handleChange} required />
                </div>
                <div>
                    <label>Password</label>
                    <input type="password" name="password" value={form.password} onChange={handleChange} required />
                </div>
                <button type="submit" disabled={status === 'loading'}>
                    {status === 'loading' ? 'Logging in...' : 'Login'}
                </button>
            </form>

            {error && <p style={{ color: 'red' }}>{error.message || 'Login failed'}</p>}

            {user?.accessToken && <p style={{ color: 'green' }}>Login successful!</p>}
        </div>
    );
};

export default Login;
