
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Index from './pages/Index';
import About from './pages/About';
import NotFound from './pages/NotFound';
import PresentationAnalyzer from './pages/PresentationAnalyzer';
import DownloadPage from './pages/DownloadPage';
import Analyzer from './pages/Analyzer';
import { Toaster } from '@/components/ui/toaster';
import './App.css';

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Index />} />
        <Route path="/about" element={<About />} />
        <Route path="/analyzer" element={<Analyzer />} />
        <Route path="/advanced-analyzer" element={<PresentationAnalyzer />} />
        <Route path="/download" element={<DownloadPage />} />
        <Route path="*" element={<NotFound />} />
      </Routes>
      <Toaster />
    </Router>
  );
}

export default App;
