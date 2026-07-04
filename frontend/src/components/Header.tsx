import React from 'react';
import { Book, Home, LogIn, LogOut } from 'lucide-react';
import { Link } from 'react-router-dom';
import BrandLogo from '@/components/BrandLogo';
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
      <header className="bg-card/80 backdrop-blur border-b border-border sticky top-0 z-10">
        <div className="container mx-auto px-4 py-3 flex items-center justify-between">
          <Link to="/" aria-label="PPT-Revive by Nucleus Digitalis — home">
            <BrandLogo />
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
                  className="relative flex items-center justify-center w-8 h-8 rounded-full bg-secondary text-secondary-foreground hover:bg-secondary/80 font-semibold"
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
                className="text-primary hover:text-primary/80 flex items-center gap-1.5"
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
