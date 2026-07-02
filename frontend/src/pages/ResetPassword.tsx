import React, { useState } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';
import { useAppDispatch, useAppSelector } from '@/hooks/reduxHooks';
import { resetPasswordAction } from '@/app-redux/auth/authActions';
import { selectStatus, selectAuthError } from '@/app-redux/auth/authSlice';
import { getErrorMessages } from '@/common/utils/getAuthErrorMessages';
import { ArrowRight, Lock, Eye, EyeOff } from 'lucide-react';

const schema = yup.object({
  password: yup.string().min(6, 'Password must be at least 6 characters').required('Password is required'),
  confirmPassword: yup
    .string()
    .oneOf([yup.ref('password')], 'Passwords must match')
    .required('Confirm Password is required'),
});

type ResetForm = {
  password: string;
  confirmPassword: string;
};

const ResetPassword: React.FC = () => {
  const [params] = useSearchParams();
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const status = useAppSelector(selectStatus);
  const error = useAppSelector(selectAuthError);
  const [message, setMessage] = useState<{ text: string; type: 'success' | 'error' } | null>(null);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirm, setShowConfirm] = useState(false);

  const userId = Number(params.get('userId'));
  const token = params.get('token') ?? '';

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ResetForm>({
    resolver: yupResolver(schema) as any,
  });

  const onSubmit = async (data: ResetForm) => {
    setMessage(null);
    const result = await dispatch(resetPasswordAction({ userId, token, password: data.password }));

    if (resetPasswordAction.fulfilled.match(result)) {
      setMessage({ text: 'Password has been reset successfully.', type: 'success' });
      setTimeout(() => navigate('/'), 3000);
    } else {
      setMessage({ text: 'Reset failed. Try again.', type: 'error' });
    }
  };

  return (
    <main className="flex-1">
      <section className="py-16 bg-gray-50">
        <div className="container mx-auto px-4">
          <div className="w-full max-w-md mx-auto bg-white p-8 rounded-xl shadow-lg">
            <h2 className="text-2xl font-bold text-center mb-6 text-gray-800">Reset Password</h2>

            {message && (
              <div
                className={`mb-4 p-3 rounded-lg ${
                  message.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'
                }`}
              >
                {message.text}
              </div>
            )}

            {!message && error && (
              <div className="p-3 rounded-lg mb-4 bg-red-100 text-red-800">
                <strong className="block mb-1">Error:</strong>
                {getErrorMessages(error).map((err, idx) => (
                  <div key={idx}>• {err}</div>
                ))}
              </div>
            )}

            <form onSubmit={handleSubmit(onSubmit)} className="space-y-5">
              {/* Password */}
              <div>
                <label htmlFor="password" className="block text-sm font-medium mb-1">New Password</label>
                <div className="relative">
                  <Lock className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                  <input
                    id="password"
                    type={showPassword ? 'text' : 'password'}
                    {...register('password')}
                    placeholder="Enter new password"
                    className="w-full pl-10 pr-10 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                  <button
                    type="button"
                    onClick={() => setShowPassword((prev) => !prev)}
                    className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-gray-600"
                  >
                    {showPassword ? <EyeOff className="h-5 w-5" /> : <Eye className="h-5 w-5" />}
                  </button>
                </div>
                {errors.password && <p className="text-sm text-red-600 mt-1">{errors.password.message}</p>}
              </div>

              {/* Confirm Password */}
              <div>
                <label htmlFor="confirmPassword" className="block text-sm font-medium mb-1">Confirm Password</label>
                <div className="relative">
                  <Lock className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                  <input
                    id="confirmPassword"
                    type={showConfirm ? 'text' : 'password'}
                    {...register('confirmPassword')}
                    placeholder="Confirm your password"
                    className="w-full pl-10 pr-10 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                  />
                  <button
                    type="button"
                    onClick={() => setShowConfirm((prev) => !prev)}
                    className="absolute right-3 top-1/2 transform -translate-y-1/2 text-gray-400 hover:text-gray-600"
                  >
                    {showConfirm ? <EyeOff className="h-5 w-5" /> : <Eye className="h-5 w-5" />}
                  </button>
                </div>
                {errors.confirmPassword && <p className="text-sm text-red-600 mt-1">{errors.confirmPassword.message}</p>}
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
                {status === 'loading' ? 'Resetting...' : 'Reset Password'}
              </button>
            </form>
          </div>
        </div>
      </section>
    </main>
  );
};

export default ResetPassword;
