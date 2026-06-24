import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { getInmuebles } from '../services/api'
import { useAuth } from '../context/AuthContext'

export default function Home() {
    const [inmuebles, setInmuebles] = useState([])
    const [loading, setLoading] = useState(true)
    const [filters, setFilters] = useState({ location: '', availableFrom: '', availableTo: '' })
    const { user, logoutUser } = useAuth()
    const navigate = useNavigate()

    useEffect(() => {
        fetchInmuebles()
    }, [])

    const fetchInmuebles = async (params = {}) => {
        setLoading(true)
        try {
            const res = await getInmuebles(params)
            setInmuebles(res.data.data || [])
        } catch (err) {
            console.error(err)
        } finally {
            setLoading(false)
        }
    }

    const handleSearch = (e) => {
        e.preventDefault()
        const params = {}
        if (filters.location) params.location = filters.location
        if (filters.availableFrom) params.availableFrom = filters.availableFrom
        if (filters.availableTo) params.availableTo = filters.availableTo
        fetchInmuebles(params)
    }

    const handleLogout = () => {
        logoutUser()
        navigate('/')
    }

    return (
        <div style={styles.page}>
            {/* Header */}
            <header style={styles.header}>
                <h1 style={styles.logo}>RentasCortas</h1>
                <nav style={styles.nav}>
                    {user ? (
                        <>
                            <span style={styles.userName}>Hola, {user.fullName?.split(' ')[0]}</span>
                            <button
                                style={styles.navBtn}
                                onClick={() => navigate(user.role === 'owner' ? '/dashboard-owner' : '/dashboard-guest')}
                            >
                                Mi cuenta
                            </button>
                            <button style={styles.navBtnOutline} onClick={handleLogout}>
                                Cerrar sesión
                            </button>
                        </>
                    ) : (
                        <>
                            <button style={styles.navBtnOutline} onClick={() => navigate('/login')}>
                                Iniciar sesión
                            </button>
                            <button style={styles.navBtn} onClick={() => navigate('/register')}>
                                Registrarse
                            </button>
                        </>
                    )}
                </nav>
            </header>

            {/* Hero + Search */}
            <div style={styles.hero}>
                <h2 style={styles.heroTitle}>Encuentra tu próximo destino</h2>
                <p style={styles.heroSubtitle}>Propiedades exclusivas para estancias inolvidables</p>

                <form onSubmit={handleSearch} style={styles.searchBar}>
                    <div style={styles.searchField}>
                        <label style={styles.searchLabel}>📍 Ubicación</label>
                        <input
                            style={styles.searchInput}
                            type="text"
                            placeholder="¿A dónde vas?"
                            value={filters.location}
                            onChange={(e) => setFilters({ ...filters, location: e.target.value })}
                        />
                    </div>
                    <div style={styles.searchDivider} />
                    <div style={styles.searchField}>
                        <label style={styles.searchLabel}>📅 Llegada</label>
                        <input
                            style={styles.searchInput}
                            type="date"
                            value={filters.availableFrom}
                            onChange={(e) => setFilters({ ...filters, availableFrom: e.target.value })}
                        />
                    </div>
                    <div style={styles.searchDivider} />
                    <div style={styles.searchField}>
                        <label style={styles.searchLabel}>📅 Salida</label>
                        <input
                            style={styles.searchInput}
                            type="date"
                            value={filters.availableTo}
                            onChange={(e) => setFilters({ ...filters, availableTo: e.target.value })}
                        />
                    </div>
                    <button type="submit" style={styles.searchBtn}>🔍</button>
                </form>
            </div>

            {/* Grid de inmuebles */}
            <main style={styles.main}>
                {loading ? (
                    <div style={styles.loading}>Cargando propiedades...</div>
                ) : inmuebles.length === 0 ? (
                    <div style={styles.empty}>
                        <p>No se encontraron propiedades con esos filtros.</p>
                        <button style={styles.clearBtn} onClick={() => { setFilters({ location: '', availableFrom: '', availableTo: '' }); fetchInmuebles() }}>
                            Limpiar filtros
                        </button>
                    </div>
                ) : (
                    <div style={styles.grid}>
                        {inmuebles.map((inmueble) => (
                            <div
                                key={inmueble.id}
                                style={styles.card}
                                onClick={() => navigate(`/inmueble/${inmueble.id}`)}
                            >
                                <div style={styles.cardImg}>
                                    {inmueble.mainImage ? (
                                        <img
                                            src={`http://localhost:8080${inmueble.mainImage}`}
                                            alt={inmueble.title}
                                            style={styles.img}
                                        />
                                    ) : (
                                        <div style={styles.imgPlaceholder}>🏠</div>
                                    )}
                                </div>
                                <div style={styles.cardBody}>
                                    <h3 style={styles.cardTitle}>{inmueble.title}</h3>
                                    <p style={styles.cardLocation}>📍 {inmueble.location}</p>
                                    <p style={styles.cardPrice}>
                                        <strong>${inmueble.pricePerNight}</strong> / noche
                                    </p>
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </main>
        </div>
    )
}

const styles = {
    page: { minHeight: '100vh', backgroundColor: '#f8f9fa' },
    header: {
        display: 'flex', justifyContent: 'space-between', alignItems: 'center',
        padding: '16px 48px', backgroundColor: 'white',
        boxShadow: '0 1px 8px rgba(0,0,0,0.06)', position: 'sticky', top: 0, zIndex: 100,
    },
    logo: { color: '#2D6A4F', fontSize: '22px', fontWeight: '700' },
    nav: { display: 'flex', alignItems: 'center', gap: '12px' },
    userName: { fontSize: '14px', color: '#555' },
    navBtn: {
        backgroundColor: '#2D6A4F', color: 'white', border: 'none',
        padding: '8px 18px', borderRadius: '8px', cursor: 'pointer', fontSize: '14px', fontWeight: '500',
    },
    navBtnOutline: {
        backgroundColor: 'transparent', color: '#2D6A4F',
        border: '1.5px solid #2D6A4F', padding: '8px 18px',
        borderRadius: '8px', cursor: 'pointer', fontSize: '14px', fontWeight: '500',
    },
    hero: {
        backgroundColor: '#2D6A4F', padding: '64px 48px 80px',
        textAlign: 'center', color: 'white',
    },
    heroTitle: { fontSize: '36px', fontWeight: '700', marginBottom: '8px' },
    heroSubtitle: { fontSize: '16px', opacity: 0.85, marginBottom: '40px' },
    searchBar: {
        display: 'flex', alignItems: 'center', backgroundColor: 'white',
        borderRadius: '16px', padding: '8px 8px 8px 24px',
        maxWidth: '720px', margin: '0 auto',
        boxShadow: '0 4px 20px rgba(0,0,0,0.15)',
    },
    searchField: { flex: 1, display: 'flex', flexDirection: 'column', padding: '4px 8px' },
    searchLabel: { fontSize: '11px', fontWeight: '600', color: '#555', marginBottom: '2px' },
    searchInput: { border: 'none', outline: 'none', fontSize: '14px', color: '#333', background: 'transparent' },
    searchDivider: { width: '1px', height: '32px', backgroundColor: '#e0e0e0', margin: '0 4px' },
    searchBtn: {
        backgroundColor: '#2D6A4F', color: 'white', border: 'none',
        borderRadius: '12px', padding: '12px 20px', fontSize: '18px', cursor: 'pointer',
    },
    main: { padding: '48px', maxWidth: '1200px', margin: '0 auto' },
    loading: { textAlign: 'center', padding: '80px', color: '#666', fontSize: '16px' },
    empty: { textAlign: 'center', padding: '80px', color: '#666' },
    clearBtn: {
        marginTop: '16px', backgroundColor: '#2D6A4F', color: 'white',
        border: 'none', padding: '10px 20px', borderRadius: '8px', cursor: 'pointer',
    },
    grid: {
        display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: '24px',
    },
    card: {
        backgroundColor: 'white', borderRadius: '16px', overflow: 'hidden',
        boxShadow: '0 2px 12px rgba(0,0,0,0.07)', cursor: 'pointer',
        transition: 'transform 0.2s, box-shadow 0.2s',
    },
    cardImg: { height: '200px', overflow: 'hidden', backgroundColor: '#f0f0f0' },
    img: { width: '100%', height: '100%', objectFit: 'cover' },
    imgPlaceholder: {
        height: '100%', display: 'flex', alignItems: 'center',
        justifyContent: 'center', fontSize: '48px', backgroundColor: '#f5f5f5',
    },
    cardBody: { padding: '16px' },
    cardTitle: { fontSize: '16px', fontWeight: '600', marginBottom: '6px', color: '#1a1a1a' },
    cardLocation: { fontSize: '13px', color: '#888', marginBottom: '10px' },
    cardPrice: { fontSize: '15px', color: '#2D6A4F' },
}