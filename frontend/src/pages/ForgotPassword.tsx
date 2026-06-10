import React, { useState } from 'react';
import { Mail, ArrowRight } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { useAppDispatch, useAppSelector } from '@/hooks/reduxHooks';
import { forgotPasswordAction } from '@/app-redux/auth/authActions';
import {
  selectStatus,
  selectAuthError,
} from '@/app-redux/auth/authSlice';
import { getErrorMessages } from '@/common/utils/getAuthErrorMessages';

type ForgotPasswordForm = {
  email: string;
};

const forgotPasswordSchema = yup.object({
  email: yup.string().required('Email is required.').email('Invalid email format.'),
});

const ForgotPassword: React.FC = () => {

  const dispatch = useAppDispatch();
  const status = useAppSelector(selectStatus);
  const error = useAppSelector(selectAuthError);
  const [message, setMessage] = useState<{ text: string; type: 'success' | 'error' } | null>(null);

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<ForgotPasswordForm>({
    resolver: yupResolver(forgotPasswordSchema) as any,
  });


  const onSubmit = async (data: ForgotPasswordForm) => {
    setMessage(null);
    const result = await dispatch(forgotPasswordAction(data));

    if (forgotPasswordAction.fulfilled.match(result)) {
      setMessage({
        text: result.payload.message ?? 'Password reset email sent. Please check your inbox.',
        type: 'success',
      });
    } else {
      setMessage({ text: 'Failed to send reset link.', type: 'error' });
    }
  };


  return (
    <main className="flex-1">
      {/* Hero section */}
      <section className="py-16 bg-gray-50">
        <div className="container mx-auto px-4">
          <div className="w-full max-w-md mx-auto bg-white p-8 rounded-xl shadow-lg">
            <h2 className="text-2xl font-bold text-center mb-6 text-gray-800">Forgot Password</h2>

            {message && (
              <div
                className={`mb-4 p-3 rounded-lg ${message.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                  }`}
              >
                {message.text}
              </div>
            )}

            {!message && error && (
              <div className="p-3 rounded-lg mb-4 bg-red-100 text-red-800">
                <strong className="block mb-1">Server Error:</strong>
                {getErrorMessages(error).map((err, idx) => (
                  <div key={idx}>• {err}</div>
                ))}
              </div>
            )}

            <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
              <div>
                <label htmlFor="email" className="block text-sm font-medium mb-1">
                  Email address
                </label>
                <div className="relative">
                  <Mail className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                  <input
                    id="email"
                    type="email"
                    placeholder="you@example.com"
                    {...register('email')}
                    className="w-full pl-10 pr-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                </div>
                {errors.email && <p className="text-sm text-red-600 mt-1">{errors.email.message}</p>}
              </div>

              <button
                type="submit"
                disabled={isSubmitting || status === 'loading'}
                className="w-full flex items-center justify-center bg-blue-600 text-white py-2 px-4 rounded-lg disabled:opacity-50"
              >
                {status === 'loading' ? (
                  <span className="inline-block h-4 w-4 border-2 border-white border-t-transparent rounded-full animate-spin mr-2" />
                ) : (
                  <ArrowRight className="h-4 w-4 mr-2" />
                )}
                {status === 'loading' ? 'Sending...' : 'Send Reset Link'}
              </button>
            </form>
          </div>
        </div>
      </section>
    </main>
  );
};

export default ForgotPassword;
