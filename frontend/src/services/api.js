import axios from 'axios'

const api = axios.create({
    baseURL: '/api',  
})

// Interceptor: agrega el token JWT automáticamente a cada request
api.interceptors.request.use((config) => {
    const token = localStorage.getItem('token')
    if (token) {
        config.headers.Authorization = `Bearer ${token}`
    }
    return config
})

// Auth
export const register = (data) => api.post('/auth/register', data)
export const login = (data) => api.post('/auth/login', data)

// Inmuebles
export const getInmuebles = (params) => api.get('/inmuebles', { params })
export const getInmueble = (id) => api.get(`/inmuebles/${id}`)
export const createInmueble = (data) => api.post('/inmuebles', data)
export const updateInmueble = (id, data) => api.put(`/inmuebles/${id}`, data)
export const deleteInmueble = (id) => api.delete(`/inmuebles/${id}`)
export const addImage = (id, formData) => api.post(`/inmuebles/${id}/images`, formData)

// Reservas
export const createReserva = (data) => api.post('/reservas', data)
export const getReservas = () => api.get('/reservas')
export const confirmReserva = (id) => api.patch(`/reservas/${id}/confirm`)
export const cancelReserva = (id) => api.patch(`/reservas/${id}/cancel`)

// Favoritos
export const getFavoritos = () => api.get('/favoritos')
export const addFavorito = (inmuebleId) => api.post(`/favoritos/${inmuebleId}`)
export const removeFavorito = (inmuebleId) => api.delete(`/favoritos/${inmuebleId}`)

// Dashboard
export const getDashboard = (params) => api.get('/dashboard', { params })

// Reportes
export const getReporte = (params) => api.get('/reportes/excel', {
    params,
    responseType: 'blob'
})

// KYC
export const validateKYC = (formData) => api.post('/kyc/validate', formData)

// Notificaciones
export const getNotificaciones = () => api.get('/notificaciones')

export default api