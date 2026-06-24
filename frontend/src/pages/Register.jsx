import { useState } from 'react'
import { useNavigate, Link } from 'react-router-dom'
import { register } from '../services/api'
import { useAuth } from '../context/AuthContext'

export default function Register() {
    const [form, setForm] = useState({
        fullName: '',
        email: '',
        phone: '',
        password: '',
        role: '',
    })
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
        if (!form.role) {
            setError('Selecciona si eres huésped o propietario')
            return
        }
        setLoading(true)
        try {
            const res = await register(form)
            const { data } = res.data
            loginUser(data, data.token)
            if (data.role === 'owner') {
                navigate('/dashboard-owner')
            } else {
                navigate('/dashboard-guest')
            }
        } catch (err) {
            setError(err.response?.data?.message || 'Error al crear la cuenta')
        } finally {
            setLoading(false)
        }
    }

    return (
        <div style={styles.container}>
            <div style={styles.card}>
                <h1 style={styles.logo}>RentasCortas</h1>
                <h2 style={styles.title}>Crea tu cuenta</h2>
                <p style={styles.subtitle}>La plataforma premium para tus estancias cortas</p>

                <div style={styles.roleSelector}>
                    <div
                        style={{
                            ...styles.roleCard,
                            ...(form.role === 'guest' ? styles.roleCardActive : {})
                        }}
                        onClick={() => setForm({ ...form, role: 'guest' })}
                    >
                        <span style={styles.roleIcon}>🧳</span>
                        <span style={styles.roleLabel}>Soy huésped</span>
                    </div>
                    <div
                        style={{
                            ...styles.roleCard,
                            ...(form.role === 'owner' ? styles.roleCardActive : {})
                        }}
                        onClick={() => setForm({ ...form, role: 'owner' })}
                    >
                        <span style={styles.roleIcon}>🏠</span>
                        <span style={styles.roleLabel}>Soy propietario</span>
                    </div>
                </div>

                <form onSubmit={handleSubmit} style={styles.form}>
                    <div style={styles.field}>
                        <label style={styles.label}>Nombre completo</label>
                        <input
                            style={styles.input}
                            type="text"
                            name="fullName"
                            placeholder="Ej. Juan Pérez"
                            value={form.fullName}
                            onChange={handleChange}
                            required
                        />
                    </div>

                    <div style={styles.field}>
                        <label style={styles.label}>Correo electrónico</label>
                        <input
                            style={styles.input}
                            type="email"
                            name="email"
                            placeholder="nombre@ejemplo.com"
                            value={form.email}
                            onChange={handleChange}
                            required
                        />
                    </div>

                    <div style={styles.row}>
                        <div style={styles.field}>
                            <label style={styles.label}>Teléfono</label>
                            <input
                                style={styles.input}
                                type="text"
                                name="phone"
                                placeholder="+57 300..."
                                value={form.phone}
                                onChange={handleChange}
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
                    </div>

                    {error && (
                        <div style={styles.error}>⚠️ {error}</div>
                    )}

                    <button
                        type="submit"
                        style={{ ...styles.button, opacity: loading ? 0.7 : 1 }}
                        disabled={loading}
                    >
                        {loading ? 'Creando cuenta...' : 'Crear cuenta →'}
                    </button>
                </form>

                <p style={styles.footer}>
                    ¿Ya tienes cuenta?{' '}
                    <Link to="/login" style={styles.link}>Inicia sesión</Link>
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
        padding: '24px',
    },
    card: {
        backgroundColor: 'white',
        padding: '48px',
        borderRadius: '16px',
        width: '100%',
        maxWidth: '480px',
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
        marginBottom: '28px',
        fontSize: '14px',
    },
    roleSelector: {
        display: 'grid',
        gridTemplateColumns: '1fr 1fr',
        gap: '12px',
        marginBottom: '28px',
    },
    roleCard: {
        border: '2px solid #e0e0e0',
        borderRadius: '12px',
        padding: '16px',
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        gap: '8px',
        cursor: 'pointer',
        transition: 'all 0.2s',
    },
    roleCardActive: {
        border: '2px solid #2D6A4F',
        backgroundColor: '#f0f7f4',
    },
    roleIcon: {
        fontSize: '28px',
    },
    roleLabel: {
        fontSize: '14px',
        fontWeight: '600',
        color: '#333',
    },
    form: {
        display: 'flex',
        flexDirection: 'column',
        gap: '20px',
    },
    row: {
        display: 'grid',
        gridTemplateColumns: '1fr 1fr',
        gap: '12px',
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