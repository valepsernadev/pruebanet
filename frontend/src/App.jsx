import { BrowserRouter, Routes, Route } from 'react-router-dom'
import Home from './pages/Home'
import Login from './pages/Login'
import Register from './pages/Register'
import DashboardOwner from './pages/DashboardOwner'
import DashboardGuest from './pages/DashboardGuest'
import InmuebleDetalle from './pages/InmuebleDetalle'
import InmuebleForm from './pages/InmuebleForm'
import KYC from './pages/KYC'

function App() {
  return (
      <BrowserRouter>
        <Routes>
            <Route path="/" element={<Home />} />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/dashboard-owner" element={<DashboardOwner />} />
            <Route path="/dashboard-guest" element={<DashboardGuest />} />
            <Route path="/inmueble/nuevo" element={<InmuebleForm />} />
            <Route path="/inmueble/editar/:id" element={<InmuebleForm />} />
            <Route path="/inmueble/:id" element={<InmuebleDetalle />} />
            <Route path="/kyc" element={<KYC />} />
        </Routes>
      </BrowserRouter>
  )
}


export default App