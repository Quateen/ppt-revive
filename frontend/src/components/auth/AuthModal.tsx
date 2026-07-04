import React, { useEffect, useState } from 'react';
import { Mail, Lock, ArrowRight, X, Eye, EyeOff } from 'lucide-react';
import { GoogleLogin, CredentialResponse } from '@react-oauth/google';
import BrandLogo from '@/components/BrandLogo';

import {
  loginAction,
  registerAction,
  verifyEmailAction,
  googleLoginAction,
} from '@/app-redux/auth/authActions';

import {
  selectAuthError,
  selectStatus,
  selectRegisterResponse,
  selectVerifyEmailResponse,
  selectUser,
  resetErrorAction,
  resetRegisterResponse,
  resetVerifyEmailResponse,
} from '@/app-redux/auth/authSlice';

import { sanitizeInput } from '@/common/utils/sanitization';
import { useAppDispatch, useAppSelector } from '@/hooks/reduxHooks';
import { getErrorMessages } from '@/common/utils/getAuthErrorMessages';
import {
  loginValidationSchema,
  signupValidationSchema,
} from '@/validation/authSchema';
import * as Yup from 'yup';

interface AuthModalProps {
  isOpen: boolean;
  onClose: () => void;
}

type AuthMode = 'signin' | 'signup';

const AuthModal: React.FC<AuthModalProps> = ({ isOpen, onClose }) => {
  const dispatch = useAppDispatch();

  const status = useAppSelector(selectStatus);
  const apiError = useAppSelector(selectAuthError);
  const registerResponse = useAppSelector(selectRegisterResponse);
  const verifyEmailResponse = useAppSelector(selectVerifyEmailResponse);
  const loginData = useAppSelector(selectUser);

  const initialFormData = {
    name: '',
    email: '',
    password: '',
    confirmPassword: '',
  };

  const [authMode, setAuthMode] = useState<AuthMode>('signin');
  const [formData, setFormData] = useState(initialFormData);
  const [formErrorMap, setFormErrorMap] = useState<Record<string, string>>({});
  const [message, setMessage] = useState<{ text: string; type: 'success' | 'error' } | null>(null);
  const [verificationCode, setVerificationCode] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setConfirmShowPassword] = useState(false);

  const isSubmitting = status === 'loading';

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { id, value } = e.target;
    setFormData((prev) => ({ ...prev, [id]: value }));
  };

  useEffect(() => {
    if (!isOpen) {
      dispatch(resetErrorAction());
      dispatch(resetRegisterResponse());
      dispatch(resetVerifyEmailResponse());
      setFormData(initialFormData);
      setVerificationCode('');
      setMessage(null);
      setFormErrorMap({});
      setShowPassword(false);
      setConfirmShowPassword(false);
      setAuthMode('signin');
    }
  }, [isOpen, dispatch]);

  useEffect(() => {
    if (registerResponse?.status && registerResponse.data) {
      setMessage({ text: registerResponse.message, type: 'success' });
    }
  }, [registerResponse]);

  useEffect(() => {
    if (verifyEmailResponse?.status) {
      dispatch(resetVerifyEmailResponse());
      setMessage({ text: 'Email verified. Please log in.', type: 'success' });
      setAuthMode('signin');
    }
  }, [verifyEmailResponse, dispatch]);

  useEffect(() => {
    if (loginData) {
      onClose();
    }
  }, [loginData, onClose]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setMessage(null);
    setFormErrorMap({});

    const sanitized = {
      email: sanitizeInput(formData.email),
      password: sanitizeInput(formData.password),
      confirmPassword: sanitizeInput(formData.confirmPassword),
      name: sanitizeInput(formData.name),
    };

    try {
      if (authMode === 'signup') {
        await signupValidationSchema.validate(sanitized, { abortEarly: false });

        const resultAction = await dispatch(
          registerAction({
            name: sanitized.name || 'User',
            email: sanitized.email,
            password: sanitized.password,
          })
        );

        if (registerAction.fulfilled.match(resultAction)) {
          setFormData((prev) => ({
            ...prev,
            password: '',
            confirmPassword: '',
          }));
        }

        return;
      }

      await loginValidationSchema.validate(sanitized, { abortEarly: false });

      const loginPayload = {
        email: sanitized.email,
        password: sanitized.password,
        ipAddress: '127.0.0.1',
        clientInformation: navigator.userAgent,
        currentBrowserTimeZone: Intl.DateTimeFormat().resolvedOptions().timeZone,
      };

      await dispatch(loginAction(loginPayload));
    } catch (err) {
      if (err instanceof Yup.ValidationError) {
        const errorMap: Record<string, string> = {};
        err.inner.forEach((e) => {
          if (e.path) errorMap[e.path] = e.message;
        });
        setFormErrorMap(errorMap);
      }
    }
  };

  const handleVerifyCodeSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!registerResponse?.data) return;

    await dispatch(
      verifyEmailAction({
        userId: registerResponse.data,
        code: verificationCode,
      })
    );
  };

  const handleGoogleLogin = async (credentialResponse: CredentialResponse) => {
    const idToken = credentialResponse.credential;
    if (!idToken) return;

    const timezone = Intl.DateTimeFormat().resolvedOptions().timeZone;

    // Decode the ID token to extract Google profile info
    const payload = JSON.parse(atob(idToken.split('.')[1]));

    const googleLoginPayload = {
      socialLogin: 'google',
      givenName: payload.given_name || '',
      familyName: payload.family_name || '',
      googleName: payload.name || '',
      googleLocal: payload.locale || '',
      googlePicture: payload.picture || '',
      googleAud: payload.aud || '',
      googleAzp: payload.azp || '',
      googleExp: payload.exp?.toString() || '',
      googleIat: payload.iat?.toString() || '',
      googleIss: payload.iss || '',
      googleSub: payload.sub || '',
      email: payload.email || '',
      idToken: idToken,
      deviceToken: 'browser',
      currentBrowserTimeZone: timezone,
    };

    await dispatch(googleLoginAction(googleLoginPayload));
  };

  const switchAuthMode = (mode) => {
    setAuthMode(mode);
    setFormData(initialFormData);
    setShowPassword(false);
    setConfirmShowPassword(false);
    setFormErrorMap({});
    dispatch(resetErrorAction());
  };


  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center">
      <div className="absolute inset-0 bg-black bg-opacity-50" onClick={onClose} />
      <div className="relative w-full max-w-md p-6 rounded-xl shadow-2xl bg-white">
        <button onClick={onClose} className="absolute top-4 right-4 text-gray-500 hover:text-gray-700">
          <X className="h-6 w-6" />
        </button>

        <div className="flex items-center justify-center mb-2">
          <BrandLogo />
        </div>

        <p className="text-center text-gray-600 mb-2">
          {authMode === 'signin' ? 'Sign in to your account' : 'Create a new account'}
        </p>

        <div className="flex bg-gray-100 mb-6 p-1 rounded-lg">
          <button onClick={() => switchAuthMode('signin')} className={`flex-1 py-2 text-sm font-medium rounded-md transition-colors duration-200 ${authMode === 'signin' ? 'bg-white text-gray-900 shadow' : 'text-gray-500 hover:text-gray-900'}`}>
            Sign In
          </button>
          <button onClick={() => switchAuthMode('signup')} className={`flex-1 py-2 text-sm font-medium rounded-md transition-colors duration-200 ${authMode === 'signup' ? 'bg-white text-gray-900 shadow' : 'text-gray-500 hover:text-gray-900'}`}>
            Sign Up
          </button>
        </div>

        {!message && apiError && (
          <div className="p-3 rounded-lg mb-4 bg-red-100 text-red-800">
            {/* <strong className="block mb-1">Server Error:</strong> */}
            {getErrorMessages(apiError).map((err, idx) => (
              <div key={idx}>{err}</div>
            ))}
          </div>
        )}

        {message && (
          <div className={`p-3 rounded-lg mb-4 ${message.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
            {message.text}
          </div>
        )}

        {authMode === 'signup' && registerResponse?.status && registerResponse.data && !verifyEmailResponse ? (
          <form onSubmit={handleVerifyCodeSubmit}>
            <label htmlFor="code" className="block text-sm font-medium mb-1">Verification Code</label>
            <input
              id="code"
              value={verificationCode}
              onChange={(e) => setVerificationCode(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded-lg"
              placeholder="Enter verification code"
            />
            <button type="submit" disabled={isSubmitting} className="mt-3 w-full bg-blue-600 text-white py-2 rounded-lg">
              {isSubmitting ? 'Verifying...' : 'Verify Email'}
            </button>
          </form>
        ) : (
          <form onSubmit={handleSubmit}>
            {authMode === 'signup' && (
              <div className="mb-4">
                <label htmlFor="name" className="block text-sm font-medium mb-1">Full Name</label>
                <input
                  id="name"
                  type="text"
                  value={formData.name}
                  onChange={handleChange}
                  className="w-full px-3 py-2 border border-gray-300 rounded-lg"
                  placeholder="Enter your full name"
                />
                {formErrorMap.name && <p className="text-sm text-red-600 mt-1">{formErrorMap.name}</p>}
              </div>
            )}

            <div className="mb-4">
              <label htmlFor="email" className="block text-sm font-medium mb-1">Email Address</label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <Mail className="h-5 w-5 text-gray-400" />
                </div>
                <input
                  id="email"
                  type="email"
                  value={formData.email}
                  onChange={handleChange}
                  className="w-full pl-10 pr-3 py-2 border border-gray-300 rounded-lg"
                  placeholder="Enter your email"
                />
              </div>
              {formErrorMap.email && <p className="text-sm text-red-600 mt-1">{formErrorMap.email}</p>}
            </div>

            <div className="mb-4">
              <label htmlFor="password" className="block text-sm font-medium mb-1">Password</label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                  <Lock className="h-5 w-5 text-gray-400" />
                </div>
                <input
                  id="password"
                  type={showPassword ? 'text' : 'password'}
                  value={formData.password}
                  onChange={handleChange}
                  className="w-full pl-10 pr-3 py-2 border border-gray-300 rounded-lg"
                  placeholder="Enter your password"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword((prev) => !prev)}
                  className="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-400 hover:text-gray-600"
                >
                  {showPassword ? <EyeOff className="h-5 w-5" /> : <Eye className="h-5 w-5" />}
                </button>
              </div>
              {formErrorMap.password && <p className="text-sm text-red-600 mt-1">{formErrorMap.password}</p>}
            </div>

            {authMode === 'signin' && (
              <div className="mb-4 text-right">
                <button
                  type="button"
                  onClick={() => {
                    onClose(); // Close modal before redirect
                    window.location.href = '/forgot-password';
                  }}
                  className="text-sm text-blue-600 hover:underline"
                >
                  Forgot Password?
                </button>
              </div>
            )}

            {authMode === 'signup' && (
              <div className="mb-4">
                <label htmlFor="confirmPassword" className="block text-sm font-medium mb-1">Confirm Password</label>
                <div className="relative">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <Lock className="h-5 w-5 text-gray-400" />
                  </div>
                  <input
                    id="confirmPassword"
                    type={showConfirmPassword ? 'text' : 'password'}
                    value={formData.confirmPassword}
                    onChange={handleChange}
                    className="w-full pl-10 pr-3 py-2 border border-gray-300 rounded-lg"
                    placeholder="Confirm your password"
                  />
                  <button
                    type="button"
                    onClick={() => setConfirmShowPassword((prev) => !prev)}
                    className="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-400 hover:text-gray-600"
                  >
                    {showConfirmPassword ? <EyeOff className="h-5 w-5" /> : <Eye className="h-5 w-5" />}
                  </button>
                </div>
                {formErrorMap.confirmPassword && <p className="text-sm text-red-600 mt-1">{formErrorMap.confirmPassword}</p>}
              </div>
            )}

            <button
              type="submit"
              disabled={isSubmitting}
              className="w-full flex items-center justify-center bg-blue-600 text-white py-2 px-4 rounded-lg disabled:opacity-50"
            >
              {isSubmitting ? (
                <span className="inline-block h-4 w-4 border-2 border-white border-t-transparent rounded-full animate-spin mr-2"></span>
              ) : (
                <ArrowRight className="h-4 w-4 mr-2" />
              )}
              {isSubmitting ? 'Processing...' : authMode === 'signup' ? 'Create Account' : 'Sign In'}
            </button>
          </form>
        )}

        <div className="mt-4 w-full flex justify-center">
          <GoogleLogin
            onSuccess={handleGoogleLogin}
            onError={() => setMessage({ text: 'Google login failed. Try again.', type: 'error' })}
            useOneTap
            theme="outline"
            size="large"
            text="continue_with"
            // width="100%"
          />
        </div>
      </div>
    </div>
  );
};

export default AuthModal;
