import { AppDispatch, RootState } from '@/app-redux/store';
import { TypedUseSelectorHook, useDispatch, useSelector } from 'react-redux';

// Typed versions of useDispatch and useSelector
export const useAppDispatch = () => useDispatch<AppDispatch>();
export const useAppSelector: TypedUseSelectorHook<RootState> = useSelector;
