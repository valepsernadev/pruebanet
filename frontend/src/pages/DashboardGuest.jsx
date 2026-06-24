import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { getReservas, getFavoritos, removeFavorito } from '../services/api'
import { useAuth } from '../context/AuthContext'

export default function DashboardGuest() {
    const { user, logoutUser } = useAuth()
    const navigate = useNavigate()
    const [tab, setTab] = useState('reservas')
    const [reservas, setReservas] = useState([])
    const [favoritos, setFavoritos] = useState([])
    const [loading, setLoading] = useState(true)

    useEffect(() => {
        if (!user || user.role !== 'guest') { navigate('/login'); return }
        fetchData()
    }, [])

    const fetchData = async () => {
        setLoading(true)
        try {
            const [resReservas, resFavoritos] = await Promise.all([
                getReservas(),
                getFavoritos()
            ])
            setReservas(resReservas.data.data || [])
            setFavoritos(resFavoritos.data.data || [])
        } catch (err) {
            console.error(err)
        } finally {
            setLoading(false)
        }
    }

    const handleLogout = () => {
        logoutUser()
        navigate('/')
    }

    const handleQuitarFavorito = async (inmuebleId) => {
        try {
            await removeFavorito(inmuebleId)
            setFavoritos(favoritos.filter(f => f.inmuebleId !== inmuebleId))
        } catch {}
    }

    const statusConfig = {
        pending:   { label: 'Pendiente',   color: '#f39c12', bg: '#fff8e1' },
        confirmed: { label: 'Confirmado',  color: '#27ae60', bg: '#f0fff4' },
        cancelled: { label: 'Cancelado',   color: '#e74c3c', bg: '#fff0f0' },
        completed: { label: 'Completado',  color: '#666',    bg: '#f5f5f5' },
    }

    return (
        <div style={styles.page}>
            {/* Header */}
            <header style={styles.header}>
                <h1 style={styles.logo} onClick={() => navigate('/')}>RentasCortas</h1>
                <div style={styles.headerRight}>
                    <span style={styles.userName}>Hola, {user?.fullName?.split(' ')[0]}</span>
                    <button style={styles.logoutBtn} onClick={handleLogout}>Cerrar sesión</button>
                </div>
            </header>

            <div style={styles.container}>
                <h2 style={styles.pageTitle}>Mi Cuenta</h2>
                <p style={styles.pageSubtitle}>Gestiona tus reservas y descubre tus lugares favoritos.</p>

                {/* Tabs */}
                <div style={styles.tabs}>
                    <button
                        style={{ ...styles.tab, ...(tab === 'reservas' ? styles.tabActive : {}) }}
                        onClick={() => setTab('reservas')}
                    >
                        🗓 Mis Reservas
                    </button>
                    <button
                        style={{ ...styles.tab, ...(tab === 'favoritos' ? styles.tabActive : {}) }}
                        onClick={() => setTab('favoritos')}
                    >
                        ❤️ Mis Favoritos
                    </button>
                    <button style={styles.navBtnOutline} onClick={() => navigate('/')}>
                        ← Catálogo
                    </button>
                </div>

                {loading ? (
                    <div style={styles.loading}>Cargando...</div>
                ) : tab === 'reservas' ? (
                    <div style={styles.list}>
                        {reservas.length === 0 ? (
                            <div style={styles.empty}>
                                <p>No tienes reservas aún.</p>
                                <button style={styles.exploreBtn} onClick={() => navigate('/')}>
                                    Explorar propiedades
                                </button>
                            </div>
                        ) : reservas.map(reserva => {
                            const status = statusConfig[reserva.status] || statusConfig.pending
                            return (
                                <div key={reserva.id} style={styles.card}>
                                    <div style={styles.cardLeft}>
                                        <div style={styles.imgPlaceholder}>
                                            {reserva.mainImage ? (
                                                <img
                                                    src={`http://localhost:8080${reserva.mainImage}`}
                                                    alt={reserva.inmuebleTitle}
                                                    style={{ width: '80px', height: '80px', objectFit: 'cover', borderRadius: '8px' }}
                                                />
                                            ) : (
                                                '🏠'
                                            )}
                                        </div>
                                    </div>
                                    <div style={styles.cardBody}>
                                        <div style={styles.cardHeader}>
                                            <h3 style={styles.cardTitle}>{reserva.inmuebleTitle}</h3>
                                            <span style={{ ...styles.badge, color: status.color, backgroundColor: status.bg }}>
                        {status.label}
                      </span>
                                        </div>
                                        <div style={styles.cardDetails}>
                                            <span>📅 {reserva.checkIn} → {reserva.checkOut}</span>
                                            <span>🕐 Check-in: {reserva.checkInTime} | Check-out: {reserva.checkOutTime}</span>
                                            <span style={styles.price}>💰 Total: ${reserva.totalPrice}</span>
                                        </div>
                                    </div>
                                </div>
                            )
                        })}
                    </div>
                ) : (
                    <div style={styles.grid}>
                        {favoritos.length === 0 ? (
                            <div style={styles.empty}>
                                <p>No tienes favoritos aún.</p>
                                <button style={styles.exploreBtn} onClick={() => navigate('/')}>
                                    Explorar propiedades
                                </button>
                            </div>
                        ) : favoritos.map(fav => (
                            <div key={fav.id} style={styles.favCard}>
                                <div style={styles.favImg}>
                                    {fav.images?.[0] ? (
                                        <img
                                            src={`http://localhost:8080${fav.images[0]}`}
                                            alt={fav.title}
                                            style={styles.img}
                                        />
                                    ) : (
                                        <div style={styles.imgPlaceholderBig}>🏠</div>
                                    )}
                                </div>
                                <div style={styles.favBody}>
                                    <h3 style={styles.favTitle}>{fav.title}</h3>
                                    <p style={styles.favLocation}>📍 {fav.location}</p>
                                    <p style={styles.favPrice}>${fav.pricePerNight} / noche</p>
                                    <div style={styles.favActions}>
                                        <button style={styles.reservarBtn} onClick={() => navigate(`/inmueble/${fav.inmuebleId}`)}>
                                            Ver y reservar
                                        </button>
                                        <button style={styles.quitarBtn} onClick={() => handleQuitarFavorito(fav.inmuebleId)}>
                                            ✕ Quitar
                                        </button>
                                    </div>
                                </div>
                            </div>
                        ))}
                    </div>
                )}
            </div>
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
    logo: { color: '#2D6A4F', fontSize: '22px', fontWeight: '700', cursor: 'pointer' },
    headerRight: { display: 'flex', alignItems: 'center', gap: '16px' },
    userName: { fontSize: '14px', color: '#555' },
    logoutBtn: {
        backgroundColor: 'transparent', border: '1.5px solid #ddd',
        padding: '8px 16px', borderRadius: '8px', cursor: 'pointer', fontSize: '14px',
    },
    container: { maxWidth: '900px', margin: '0 auto', padding: '40px 24px' },
    pageTitle: { fontSize: '28px', fontWeight: '700', marginBottom: '4px' },
    pageSubtitle: { color: '#666', marginBottom: '32px' },
    tabs: { display: 'flex', gap: '4px', borderBottom: '2px solid #eee', marginBottom: '32px' },
    tab: {
        padding: '12px 20px', border: 'none', backgroundColor: 'transparent',
        cursor: 'pointer', fontSize: '15px', color: '#666', fontWeight: '500',
    },
    tabActive: { color: '#2D6A4F', borderBottom: '2px solid #2D6A4F', marginBottom: '-2px' },
    loading: { textAlign: 'center', padding: '60px', color: '#666' },
    empty: { textAlign: 'center', padding: '60px', color: '#888' },
    exploreBtn: {
        marginTop: '16px', backgroundColor: '#2D6A4F', color: 'white',
        border: 'none', padding: '10px 24px', borderRadius: '8px', cursor: 'pointer', fontSize: '14px',
    },
    list: { display: 'flex', flexDirection: 'column', gap: '16px' },
    card: {
        backgroundColor: 'white', borderRadius: '12px', padding: '20px',
        boxShadow: '0 2px 8px rgba(0,0,0,0.06)', display: 'flex', gap: '16px',
    },
    cardLeft: {},
    imgPlaceholder: {
        width: '80px', height: '80px', backgroundColor: '#f0f0f0',
        borderRadius: '8px', display: 'flex', alignItems: 'center',
        justifyContent: 'center', fontSize: '32px',
    },
    cardBody: { flex: 1 },
    cardHeader: { display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', marginBottom: '12px' },
    cardTitle: { fontSize: '17px', fontWeight: '600' },
    badge: { padding: '4px 12px', borderRadius: '20px', fontSize: '13px', fontWeight: '600' },
    cardDetails: { display: 'flex', flexDirection: 'column', gap: '6px', fontSize: '14px', color: '#555' },
    price: { color: '#2D6A4F', fontWeight: '600' },
    grid: { display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(260px, 1fr))', gap: '20px' },
    favCard: { backgroundColor: 'white', borderRadius: '12px', overflow: 'hidden', boxShadow: '0 2px 8px rgba(0,0,0,0.06)' },
    favImg: { height: '160px', backgroundColor: '#f0f0f0' },
    img: { width: '100%', height: '100%', objectFit: 'cover' },
    imgPlaceholderBig: { height: '100%', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '48px' },
    favBody: { padding: '16px' },
    favTitle: { fontSize: '15px', fontWeight: '600', marginBottom: '4px' },
    favLocation: { fontSize: '13px', color: '#888', marginBottom: '6px' },
    favPrice: { fontSize: '14px', color: '#2D6A4F', fontWeight: '600', marginBottom: '12px' },
    favActions: { display: 'flex', gap: '8px' },
    reservarBtn: {
        flex: 1, backgroundColor: '#2D6A4F', color: 'white',
        border: 'none', padding: '8px', borderRadius: '8px', cursor: 'pointer', fontSize: '13px',
    },
    quitarBtn: {
        backgroundColor: 'transparent', border: '1.5px solid #ddd',
        padding: '8px 12px', borderRadius: '8px', cursor: 'pointer', fontSize: '13px', color: '#888',
    },
}