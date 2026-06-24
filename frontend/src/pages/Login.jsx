import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { login } from '../services/api'
import { useAuth } from '../context/AuthContext'

export default function Login() {
    const [form, setForm] = useState({ email: '', password: '' })
    const [error, setError] = useState('')
    const [loading, setLoading] = useState(false)
    const { loginUser } = useAuth()
    const navigate = useNavigate()

    const handleChange = (e) => {
        setForm({ ...form, [e.target.name]: e.target.value })
        setError('')
    }

    const handleSubmit = async (e) => {
        e.preventDefault()
        setLoading(true)
        try {
            const res = await login(form)
            const { data } = res.data
            loginUser(data, data.token)
            if (data.role === 'owner') {
                navigate('/dashboard-owner')
            } else {
                navigate('/dashboard-guest')
            }
        } catch (err) {
            const status = err.response?.status
            const message = err.response?.data?.message || ''

            if (status === 401) {
                setError('Correo o contraseña incorrectos. ¿Ya tienes cuenta?')
            } else if (status === 404 || message.toLowerCase().includes('no existe')) {
                setError('No encontramos una cuenta con ese correo.')
                setTimeout(() => navigate('/register'), 2000)
            } else {
                setError('Ocurrió un error. Intenta de nuevo.')
            }
        } finally {
            setLoading(false)
        }
    }

    return (
        <div style={styles.container}>
            <div style={styles.card}>
                <h1 style={styles.logo}>RentasCortas</h1>
                <h2 style={styles.title}>Bienvenido de nuevo</h2>
                <p style={styles.subtitle}>Accede a tu cuenta para gestionar tus estancias</p>

                <form onSubmit={handleSubmit} style={styles.form}>
                    <div style={styles.field}>
                        <label style={styles.label}>Correo electrónico</label>
                        <input
                            style={styles.input}
                            type="email"
                            name="email"
                            placeholder="tu@email.com"
                            value={form.email}
                            onChange={handleChange}
                            required
                        />
                    </div>

                    <div style={styles.field}>
                        <label style={styles.label}>Contraseña</label>
                        <input
                            style={styles.input}
                            type="password"
                            name="password"
                            placeholder="••••••••"
                            value={form.password}
                            onChange={handleChange}
                            required
                        />
                    </div>

                    {error && (
                        <div style={styles.error}>
                            ⚠️ {error}{' '}
                            <Link to="/register" style={{ color: '#c0392b', fontWeight: '600' }}>
                                Regístrate aquí
                            </Link>
                        </div>
                    )}

                    <button
                        type="submit"
                        style={{...styles.button, opacity: loading ? 0.7 : 1}}
                        disabled={loading}
                    >
                        {loading ? 'Iniciando sesión...' : 'Iniciar sesión →'}
                    </button>
                </form>

                <p style={styles.footer}>
                    ¿No tienes cuenta?{' '}
                    <Link to="/register" style={styles.link}>Regístrate</Link>
                </p>
            </div>
        </div>
    )
}

const styles = {
    container: {
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        backgroundColor: '#f0f4f0',
    },
    card: {
        backgroundColor: 'white',
        padding: '48px',
        borderRadius: '16px',
        width: '100%',
        maxWidth: '420px',
        boxShadow: '0 4px 24px rgba(0,0,0,0.08)',
    },
    logo: {
        color: '#2D6A4F',
        fontSize: '24px',
        fontWeight: '700',
        textAlign: 'center',
        marginBottom: '24px',
    },
    title: {
        fontSize: '22px',
        fontWeight: '700',
        textAlign: 'center',
        marginBottom: '8px',
    },
    subtitle: {
        color: '#666',
        textAlign: 'center',
        marginBottom: '32px',
        fontSize: '14px',
    },
    form: {
        display: 'flex',
        flexDirection: 'column',
        gap: '20px',
    },
    field: {
        display: 'flex',
        flexDirection: 'column',
        gap: '6px',
    },
    label: {
        fontSize: '14px',
        fontWeight: '500',
        color: '#333',
    },
    input: {
        padding: '12px 16px',
        borderRadius: '8px',
        border: '1px solid #ddd',
        fontSize: '15px',
        outline: 'none',
        transition: 'border 0.2s',
    },
    error: {
        backgroundColor: '#fff0f0',
        color: '#c0392b',
        padding: '12px 16px',
        borderRadius: '8px',
        fontSize: '14px',
        border: '1px solid #ffd0d0',
    },
    button: {
        backgroundColor: '#2D6A4F',
        color: 'white',
        padding: '14px',
        borderRadius: '8px',
        border: 'none',
        fontSize: '16px',
        fontWeight: '600',
        cursor: 'pointer',
        marginTop: '8px',
    },
    footer: {
        textAlign: 'center',
        marginTop: '24px',
        fontSize: '14px',
        color: '#666',
    },
    link: {
        color: '#2D6A4F',
        fontWeight: '600',
    },
}