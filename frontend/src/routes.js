import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Home from './pages/home';
import Login from './pages/login';
import Results from './pages/results'
// Import other pages as needed

const AppRoutes = () => {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/login" element={<Login />} />
        <Route path="/results" element={<Results />} />
        {/* Add more routes as needed */}
      </Routes>
    </BrowserRouter>
  );
};

export default AppRoutes;