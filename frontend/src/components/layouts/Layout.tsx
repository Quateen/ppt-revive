// Layout.tsx
import { Outlet } from 'react-router-dom';
import Header from '@/components/Header';
import Footer from '@/components/Footer';

const Layout = () => (
    <div className="flex flex-col min-h-screen">
        <Header />
        <main className="flex-1">
            <Outlet /> {/* Nested routes rendered here */}
        </main>
        <Footer />
    </div>
);

export default Layout;
