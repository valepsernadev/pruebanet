import { useState, useEffect } from 'react'
import { useParams, useNavigate } from 'react-router-dom'
import { getInmueble, createReserva, addFavorito, removeFavorito, getFavoritos } from '../services/api'
import { useAuth } from '../context/AuthContext'

export default function InmuebleDetalle() {
    const { id } = useParams()
    const { user } = useAuth()
    const navigate = useNavigate()
    const [inmueble, setInmueble] = useState(null)
    const [loading, setLoading] = useState(true)
    const [isFavorito, setIsFavorito] = useState(false)
    const [fechas, setFechas] = useState({ checkIn: '', checkOut: '' })
    const [reservando, setReservando] = useState(false)
    const [mensaje, setMensaje] = useState({ texto: '', tipo: '' })
    const [imgActiva, setImgActiva] = useState(0)
    const hoy = new Date().toISOString().split('T')[0]
    const pasadoMañana = new Date(Date.now() + 2 * 24 * 60 * 60 * 1000).toISOString().split('T')[0]

    useEffect(() => {
        fetchInmueble()
        if (user?.role === 'guest') checkFavorito()
    }, [id])

    const fetchInmueble = async () => {
        try {
            const res = await getInmueble(id)
            setInmueble(res.data.data)
        } catch {
            navigate('/')
        } finally {
            setLoading(false)
        }
    }

    const checkFavorito = async () => {
        try {
            const res = await getFavoritos()
            const favs = res.data.data || []
            setIsFavorito(favs.some(f => f.inmuebleId === id))
        } catch {}
    }

    const toggleFavorito = async () => {
        if (!user) { navigate('/login'); return }
        try {
            if (isFavorito) {
                await removeFavorito(id)
                setIsFavorito(false)
            } else {
                await addFavorito(id)
                setIsFavorito(true)
            }
        } catch (err) {
            setMensaje({ texto: err.response?.data?.message || 'Error', tipo: 'error' })
        }
    }

    const calcularNoches = () => {
        if (!fechas.checkIn || !fechas.checkOut) return 0
        const diff = new Date(fechas.checkOut) - new Date(fechas.checkIn)
        return Math.max(0, Math.floor(diff / (1000 * 60 * 60 * 24)))
    }

    const handleReservar = async () => {
        if (!user) { navigate('/login'); return }
        if (!fechas.checkIn || !fechas.checkOut) {
            setMensaje({ texto: 'Selecciona las fechas de check-in y check-out', tipo: 'error' })
            return
        }
        setReservando(true)
        try {
            await createReserva({ inmuebleId: id, checkIn: fechas.checkIn, checkOut: fechas.checkOut })
            setMensaje({ texto: '¡Reserva creada exitosamente! Pendiente de confirmación por el propietario.', tipo: 'success' })
        } catch (err) {
            setMensaje({ texto: err.response?.data?.message || 'Error al crear la reserva', tipo: 'error' })
        } finally {
            setReservando(false)
        }
    }

    if (loading) return <div style={styles.loading}>Cargando...</div>
    if (!inmueble) return null

    const noches = calcularNoches()
    const total = noches * inmueble.pricePerNight

    return (
        <div style={styles.page}>
            {/* Header */}
            <header style={styles.header}>
                <h1 style={styles.logo} onClick={() => navigate('/')}>RentasCortas</h1>
                <button style={styles.backBtn} onClick={() => navigate('/')}>← Volver al catálogo</button>
            </header>

            <div style={styles.container}>
                <h2 style={styles.title}>{inmueble.title}</h2>
                <p style={styles.location}>📍 {inmueble.location}</p>

                {/* Galería */}
                {inmueble.images?.length > 0 && (
                    <div style={styles.gallery}>
                        <img
                            src={`http://localhost:8080${inmueble.images[imgActiva].imageUrl}`}
                            alt={inmueble.title}
                            style={styles.mainImg}
                        />
                        {inmueble.images.length > 1 && (
                            <div style={styles.thumbnails}>
                                {inmueble.images.map((img, i) => (
                                    <img
                                        key={img.id}
                                        src={`http://localhost:8080${img.imageUrl}`}
                                        alt=""
                                        style={{
                                            ...styles.thumb,
                                            ...(i === imgActiva ? styles.thumbActive : {})
                                        }}
                                        onClick={() => setImgActiva(i)}
                                    />
                                ))}
                            </div>
                        )}
                    </div>
                )}

                <div style={styles.content}>
                    {/* Info */}
                    <div style={styles.info}>
                        <div style={styles.hostRow}>
                            <div>
                                <h3 style={styles.hostTitle}>Anfitrión: {inmueble.ownerName}</h3>
                            </div>
                            <button
                                style={{ ...styles.favBtn, color: isFavorito ? '#e74c3c' : '#aaa' }}
                                onClick={toggleFavorito}
                            >
                                {isFavorito ? '❤️ Guardado' : '🤍 Guardar'}
                            </button>
                        </div>

                        <p style={styles.description}>{inmueble.description}</p>
                    </div>

                    {/* Panel de reserva */}
                    <div style={styles.bookingPanel}>
                        <div style={styles.priceRow}>
                            <span style={styles.price}>${inmueble.pricePerNight}</span>
                            <span style={styles.perNight}> / noche</span>
                        </div>

                        <div style={styles.dateFields}>
                            <div style={styles.dateField}>
                                <label style={styles.dateLabel}>LLEGADA</label>
                                <input
                                    style={styles.dateInput}
                                    type="date"
                                    min={pasadoMañana}
                                    value={fechas.checkIn}
                                    onChange={(e) => setFechas({ ...fechas, checkIn: e.target.value })}
                                />
                            </div>
                            <div style={styles.dateField}>
                                <label style={styles.dateLabel}>SALIDA</label>
                                <input
                                    style={styles.dateInput}
                                    type="date"
                                    min={fechas.checkIn || pasadoMañana}
                                    value={fechas.checkOut}
                                    onChange={(e) => setFechas({ ...fechas, checkOut: e.target.value })}
                                />
                            </div>
                        </div>

                        {noches > 0 && (
                            <div style={styles.priceBreakdown}>
                                <div style={styles.priceRow2}>
                                    <span>${inmueble.pricePerNight} × {noches} noches</span>
                                    <span>${total.toFixed(2)}</span>
                                </div>
                                <div style={{ ...styles.priceRow2, fontWeight: '700', borderTop: '1px solid #eee', paddingTop: '12px', marginTop: '8px' }}>
                                    <span>Total</span>
                                    <span>${total.toFixed(2)}</span>
                                </div>
                            </div>
                        )}

                        {mensaje.texto && (
                            <div style={mensaje.tipo === 'success' ? styles.success : styles.error}>
                                {mensaje.tipo === 'success' ? '✅' : '⚠️'} {mensaje.texto}
                            </div>
                        )}

                        <button
                            style={{ ...styles.reservarBtn, opacity: reservando ? 0.7 : 1 }}
                            onClick={handleReservar}
                            disabled={reservando}
                        >
                            {reservando ? 'Procesando...' : user ? 'Reservar ahora' : 'Inicia sesión para reservar'}
                        </button>

                        <p style={styles.noCharge}>No se te cobrará nada todavía</p>
                    </div>
                </div>
            </div>
        </div>
    )
}

const styles = {
    page: { minHeight: '100vh', backgroundColor: '#f8f9fa' },
    loading: { textAlign: 'center', padding: '80px', fontSize: '16px', color: '#666' },
    header: {
        display: 'flex', justifyContent: 'space-between', alignItems: 'center',
        padding: '16px 48px', backgroundColor: 'white',
        boxShadow: '0 1px 8px rgba(0,0,0,0.06)',
    },
    logo: { color: '#2D6A4F', fontSize: '22px', fontWeight: '700', cursor: 'pointer' },
    backBtn: {
        backgroundColor: 'transparent', border: '1.5px solid #ddd',
        padding: '8px 16px', borderRadius: '8px', cursor: 'pointer', fontSize: '14px',
    },
    container: { maxWidth: '1100px', margin: '0 auto', padding: '32px 48px' },
    title: { fontSize: '28px', fontWeight: '700', marginBottom: '8px' },
    location: { color: '#666', fontSize: '15px', marginBottom: '24px' },
    gallery: { marginBottom: '32px' },
    mainImg: { width: '100%', height: '400px', objectFit: 'cover', borderRadius: '16px' },
    thumbnails: { display: 'flex', gap: '8px', marginTop: '8px' },
    thumb: { width: '80px', height: '60px', objectFit: 'cover', borderRadius: '8px', cursor: 'pointer', opacity: 0.7 },
    thumbActive: { opacity: 1, outline: '2px solid #2D6A4F' },
    content: { display: 'grid', gridTemplateColumns: '1fr 380px', gap: '48px', alignItems: 'start' },
    info: {},
    hostRow: { display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' },
    hostTitle: { fontSize: '18px', fontWeight: '600' },
    favBtn: { background: 'none', border: '1.5px solid #ddd', padding: '8px 16px', borderRadius: '8px', cursor: 'pointer', fontSize: '14px' },
    description: { color: '#444', lineHeight: '1.7', fontSize: '15px' },
    bookingPanel: {
        backgroundColor: 'white', borderRadius: '16px', padding: '28px',
        boxShadow: '0 4px 20px rgba(0,0,0,0.1)', position: 'sticky', top: '80px',
    },
    priceRow: { marginBottom: '20px' },
    price: { fontSize: '24px', fontWeight: '700' },
    perNight: { fontSize: '16px', color: '#666' },
    dateFields: { display: 'grid', gridTemplateColumns: '1fr 1fr', border: '1px solid #ddd', borderRadius: '8px', overflow: 'hidden', marginBottom: '16px' },
    dateField: { padding: '12px', borderRight: '1px solid #ddd' },
    dateLabel: { fontSize: '10px', fontWeight: '700', color: '#555', display: 'block', marginBottom: '4px' },
    dateInput: { border: 'none', outline: 'none', fontSize: '14px', width: '100%' },
    priceBreakdown: { marginBottom: '16px' },
    priceRow2: { display: 'flex', justifyContent: 'space-between', fontSize: '15px', marginBottom: '8px' },
    success: { backgroundColor: '#f0fff4', color: '#27ae60', padding: '12px', borderRadius: '8px', fontSize: '14px', marginBottom: '12px' },
    error: { backgroundColor: '#fff0f0', color: '#c0392b', padding: '12px', borderRadius: '8px', fontSize: '14px', marginBottom: '12px' },
    reservarBtn: {
        width: '100%', backgroundColor: '#2D6A4F', color: 'white',
        border: 'none', padding: '14px', borderRadius: '8px',
        fontSize: '16px', fontWeight: '600', cursor: 'pointer',
    },
    noCharge: { textAlign: 'center', fontSize: '13px', color: '#888', marginTop: '8px' },
}