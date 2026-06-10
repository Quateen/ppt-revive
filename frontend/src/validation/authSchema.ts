import { PasswordForm } from '@/app-redux/auth/authTypes';
import * as yup from 'yup';

export const signupValidationSchema = yup.object({
  name: yup
    .string()
    .required('Name is required.')
    .min(2, 'Name must be at least 2 characters.'),

  email: yup
    .string()
    .required('Email is required.')
    .email('Invalid email format.'),

  password: yup
    .string()
    .required('Password is required.')
    .min(8, 'Password must be at least 8 characters long.')
    .matches(/[A-Z]/, 'Password must contain at least one uppercase letter.')
    .matches(/[a-z]/, 'Password must contain at least one lowercase letter.')
    .matches(/[!@#$%^&*()\-+]/, 'Password must contain at least one special character (!@#$%^&*()-+).'),

  confirmPassword: yup
    .string()
    .oneOf([yup.ref('password')], 'Passwords must match.')
    .required('Confirm password is required.'),
});

export const loginValidationSchema = yup.object({
  email: yup
    .string()
    .required('Email is required.')
    .email('Invalid email format.'),

  password: yup
    .string()
    .required('Password is required.'),
});

export const changePasswordSchema: yup.ObjectSchema<PasswordForm> = yup.object({
  oldPassword: yup.string().required('Current password is required'),
  newPassword: yup.string().required('New password is required').min(6, 'Minimum 6 characters.'),
  confirmPassword: yup.string()
    .required('Confirm your password.')
    .oneOf([yup.ref('newPassword')], 'Passwords do not match.'),
});