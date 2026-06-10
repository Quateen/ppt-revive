import React, { useState } from 'react';
import { Lock, ArrowRight, Eye, EyeOff } from 'lucide-react';
import { useAppDispatch, useAppSelector } from '@/hooks/reduxHooks';
import { updatePasswordAction } from '@/app-redux/auth/authActions';
import { selectStatus, selectAuthError, selectUser } from '@/app-redux/auth/authSlice';
import { getErrorMessages } from '@/common/utils/getAuthErrorMessages';
import { useForm } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import { changePasswordSchema } from '@/validation/authSchema';
import { PasswordForm } from '@/app-redux/auth/authTypes';
import { getUserInitials } from '@/common/utils/utility';



const Profile: React.FC = () => {
    const dispatch = useAppDispatch();
    const status = useAppSelector(selectStatus);
    const error = useAppSelector(selectAuthError);
    const isSubmitting = status === 'loading';
    const user = useAppSelector(selectUser);

    const {
        register,
        handleSubmit,
        reset,
        formState: { errors },
    } = useForm<PasswordForm>({
        resolver: yupResolver(changePasswordSchema) as any,
    });

    const passwordFields = [
        { name: 'oldPassword', label: 'Current Password' },
        { name: 'newPassword', label: 'New Password' },
        { name: 'confirmPassword', label: 'Confirm Password' },
    ];

    const [message, setMessage] = useState<{ text: string; type: 'success' | 'error' } | null>(null);
    const [passwordVisibility, setPasswordVisibility] = useState<Record<string, boolean>>({});

    const toggleVisibility = (field: string) => {
        setPasswordVisibility((prev) => ({
            ...prev,
            [field]: !prev[field],
        }));
    };

    const onSubmit = async (data: PasswordForm) => {
        setMessage(null);

        const result = await dispatch(
            updatePasswordAction({ oldPassword: data.oldPassword, newPassword: data.newPassword })
        );

        if (updatePasswordAction.fulfilled.match(result)) {
            setMessage({ text: 'Password updated successfully!', type: 'success' });
            reset();
        } else {
            setMessage({ text: 'Failed to update password.', type: 'error' });
        }
    };

    return (
        <div className="max-w-5xl mx-auto px-2 md:px-6 py-10 grid grid-cols-1 md:grid-cols-2 gap-10">
            {/* Avatar Section */}
            {user.name && (
                <div className="flex flex-col items-center justify-center text-center">
                    <div className="relative w-32 h-32 rounded-full bg-gray-200 shadow-md flex items-center justify-center text-4xl font-semibold text-white">
                        {/* Background Image */}
                        <img
                            src="https://lh3.googleusercontent.com/a/default-user-avatar.png"
                            alt="User Avatar"
                            className="absolute inset-0 w-full h-full object-cover rounded-full"
                            onError={(e) => {
                                (e.target as HTMLImageElement).style.display = 'none';
                            }}
                        />
                        {/* Initials Fallback */}
                        <span className="z-10">{getUserInitials(user.name)}</span>
                    </div>

                    <div className="mt-4">
                        <h3 className="text-xl font-semibold text-gray-800">{user.name}</h3>
                        <p className="text-gray-500 text-sm">{user.email}</p>
                    </div>
                </div>
            )}


            {/* Change Password Form */}
            <div>
                <h2 className="text-xl font-semibold text-gray-800 mb-4">Change Password</h2>

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
                    {passwordFields.map(({ name, label }) => (
                        <div key={name} className="mb-4">
                            <label htmlFor={name} className="block text-sm font-medium mb-1">
                                {label}
                            </label>
                            <div className="relative">
                                <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                                    <Lock className="h-5 w-5 text-gray-400" />
                                </div>
                                <input
                                    id={name}
                                    type={passwordVisibility[name] ? 'text' : 'password'}
                                    {...register(name as keyof PasswordForm)}
                                    className="w-full pl-10 pr-3 py-2 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-blue-500"
                                    placeholder={label}
                                />
                                <button
                                    type="button"
                                    onClick={() => toggleVisibility(name)}
                                    className="absolute inset-y-0 right-0 pr-3 flex items-center text-gray-400 hover:text-gray-600"
                                >
                                    {passwordVisibility[name] ? <EyeOff className="h-5 w-5" /> : <Eye className="h-5 w-5" />}
                                </button>
                            </div>
                            {errors[name as keyof PasswordForm] && (
                                <p className="text-sm text-red-600 mt-1">
                                    {errors[name as keyof PasswordForm]?.message}
                                </p>
                            )}
                        </div>
                    ))}

                    <button
                        type="submit"
                        disabled={isSubmitting}
                        className="w-full flex items-center justify-center bg-blue-600 text-white py-2 px-4 rounded-lg disabled:opacity-50"
                    >
                        {isSubmitting ? (
                            <span className="inline-block h-4 w-4 border-2 border-white border-t-transparent rounded-full animate-spin mr-2" />
                        ) : (
                            <ArrowRight className="h-4 w-4 mr-2" />
                        )}
                        {isSubmitting ? 'Updating...' : 'Change Password'}
                    </button>
                </form>
            </div>
        </div>
    );
};

export default Profile;
