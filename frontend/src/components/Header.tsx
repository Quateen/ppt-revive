import React from 'react';
import { Book, FileText, Home, LogIn, LogOut } from 'lucide-react';
import { Link } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { useAppSelector, useAppDispatch } from '@/hooks/reduxHooks';
import { selectUser } from '@/app-redux/auth/authSlice';
import { removeCurrentUser } from '@/common/utils/userAttribs4mLocalStorage';
import { AppConfig } from '@/config';
import AuthModal from '@/components/auth/AuthModal';
import { showAuthModal, hideAuthModal } from '@/app-redux/ui/uiSlice';
import { RootState } from '@/app-redux/store';
import { getUserInitials } from '@/common/utils/utility';
import { googleLogout } from '@react-oauth/google';

const Header: React.FC = () => {
  const user = useAppSelector(selectUser);
  const visible = useAppSelector((state: RootState) => state.ui.isAuthModalVisible);
  const dispatch = useAppDispatch();

  console.log("user", user);

  const handleLogout = () => {
    removeCurrentUser(AppConfig.STORAGE_KEY);
    googleLogout();

    // window.google?.accounts?.id?.disableAutoSelect?.();
    window.location.reload();
  };

  return (
    <>
      <header className="bg-white border-b border-gray-200 sticky top-0 z-10">
        <div className="container mx-auto px-4 py-3 flex items-center justify-between">
          <Link to="/" className="flex items-center gap-2">
            <div className="bg-medical-600 p-1.5 rounded-md">
              <FileText className="w-5 h-5 text-white" />
            </div>
            <span className="font-semibold text-lg text-gray-800">MediPresent</span>
            <span className="text-medical-600 font-medium">Revive</span>
          </Link>

          <nav className="flex gap-4 items-center">
            <Button variant="ghost" size="sm" asChild>
              <Link to="/" className="flex items-center gap-1.5">
                <Home className="w-4 h-4" />
                <span>Home</span>
              </Link>
            </Button>

            <Button variant="ghost" size="sm" asChild>
              <Link to="/about" className="flex items-center gap-1.5">
                <Book className="w-4 h-4" />
                <span>About</span>
              </Link>
            </Button>

            {user ? (
              <>
                <Button
                  variant="ghost"
                  size="sm"
                  asChild
                  className="relative flex items-center justify-center w-8 h-8 rounded-full bg-blue-100 text-blue-700 hover:bg-blue-200 font-semibold"
                >
                  <Link to="/profile" title="Profile">
                    {getUserInitials(user?.name || user?.email || '')}
                  </Link>
                </Button>

                <Button
                  variant="ghost"
                  size="sm"
                  onClick={handleLogout}
                  className="text-red-600 hover:text-red-700"
                >
                  <LogOut className="w-4 h-4 mr-1" /> Logout
                </Button>
              </>
            ) : (

              <Button
                variant="ghost"
                size="sm"
                onClick={() => dispatch(showAuthModal())}
                className="text-blue-600 hover:text-blue-800 flex items-center gap-1.5"
              >
                <LogIn className="w-4 h-4" />
                <span>Login</span>
              </Button>
            )}
          </nav>
        </div>
      </header>

      <AuthModal isOpen={visible} onClose={() => dispatch(hideAuthModal())} />
    </>
  );
};

export default Header;
