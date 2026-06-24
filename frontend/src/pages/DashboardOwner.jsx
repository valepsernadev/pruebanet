import { useState, useEffect } from 'react'
import { useNavigate } from 'react-router-dom'
import { getInmuebles, deleteInmueble, getDashboard, getReporte } from '../services/api'
import { useAuth } from '../context/AuthContext'

export default function DashboardOwner() {
    const { user, logoutUser } = useAuth()
    const navigate = useNavigate()
    const [tab, setTab] = useState('inmuebles')
    const [inmuebles, setInmuebles] = useState([])
    const [metrics, setMetrics] = useState(null)
    const [loading, setLoading] = useState(true)
    const [descargando, setDescargando] = useState(false)
    const [periodo, setPeriodo] = useState({ startDate: '', endDate: '' })

    useEffect(() => {
        if (!user || user.role !== 'owner') { navigate('/login'); return }
        fetchInmuebles()
    }, [])

    useEffect(() => {
        if (tab === 'metricas') fetchMetrics()
    }, [tab])

    const fetchInmuebles = async () => {
        setLoading(true)
        try {
            const res = await getInmuebles()
            setInmuebles(res.data.data || [])
        } catch {}
        finally { setLoading(false) }
    }

    const fetchMetrics = async () => {
        setLoading(true)
        try {
            const res = await getDashboard(periodo)
            setMetrics(res.data.data)
        } catch {}
        finally { setLoading(false) }
    }

    const handleSoftDelete = async (id) => {
        if (!confirm('¿Estás seguro de eliminar este inmueble?')) return
        try {
            await deleteInmueble(id)
            setInmuebles(inmuebles.filter(i => i.id !== id))
        } catch {}
    }

    const handleDescargarReporte = async () => {
        setDescargando(true)
        try {
            const res = await getReporte(periodo)
            const url = window.URL.createObjectURL(new Blob([res.data]))
            const link = document.createElement('a')
            link.href = url
            link.setAttribute('download', `reporte-${new Date().toISOString().split('T')[0]}.xlsx`)
            document.body.appendChild(link)
            link.click()
            link.remove()
        } catch {}
        finally { setDescargando(false) }
    }

    const handleLogout = () => { logoutUser(); navigate('/') }

    const [inmuebleSeleccionado, setInmuebleSeleccionado] = useState('')

    const statusConfig = {
        active:   { label: 'Activo',    color: '#27ae60', bg: '#f0fff4' },
        inactive: { label: 'Inactivo',  color: '#f39c12', bg: '#fff8e1' },
        deleted:  { label: 'Eliminado', color: '#e74c3c', bg: '#fff0f0' },
    }

    return (
        <div style={styles.page}>
            <header style={styles.header}>
                <h1 style={styles.logo} onClick={() => navigate('/')}>RentasCortas</h1>
                <div style={styles.headerRight}>
                    <span style={styles.userName}>{user?.fullName?.split(' ')[0]}</span>
                    <button style={styles.logoutBtn} onClick={handleLogout}>Cerrar sesión</button>
                </div>
            </header>

            <div style={styles.container}>
                <h2 style={styles.pageTitle}>Panel del Propietario</h2>
                <p style={styles.pageSubtitle}>Gestiona tus propiedades y analiza el rendimiento de tus rentas.</p>

                <div style={styles.tabs}>
                    {['inmuebles', 'metricas', 'reportes'].map(t => (
                        <button
                            key={t}
                            style={{ ...styles.tab, ...(tab === t ? styles.tabActive : {}) }}
                            onClick={() => setTab(t)}
                        >
                            {t === 'inmuebles' ? '🏠 Mis Inmuebles' : t === 'metricas' ? '📊 Métricas' : '📥 Reportes'}
                        </button>
                    ))}
                </div>

                {/* INMUEBLES */}
                {tab === 'inmuebles' && (
                    <div>
                        <button style={styles.addBtn} onClick={() => navigate('/inmueble/nuevo')}>
                            + Añadir Propiedad
                        </button>
                        {loading ? <div style={styles.loading}>Cargando...</div> : (
                            <div style={styles.grid}>
                                {inmuebles.map(inmueble => {
                                    console.log('Inmueble:', inmueble.title, 'Images:', inmueble.images)
                                    const status = statusConfig[inmueble.status] || statusConfig.active
                                    return (
                                        <div key={inmueble.id} style={styles.card}>
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
                                                <span style={{ ...styles.badge, color: status.color, backgroundColor: status.bg }}>
                          {status.label}
                        </span>
                                            </div>
                                            <div style={styles.cardBody}>
                                                <h3 style={styles.cardTitle}>{inmueble.title}</h3>
                                                <p style={styles.cardLocation}>📍 {inmueble.location}</p>
                                                <p style={styles.cardPrice}>${inmueble.pricePerNight} / noche</p>
                                                <div style={styles.cardActions}>
                                                    <button style={styles.editBtn} onClick={() => navigate(`/inmueble/editar/${inmueble.id}`)}>
                                                        ✏️ Editar
                                                    </button>
                                                    <button style={styles.deleteBtn} onClick={() => handleSoftDelete(inmueble.id)}>
                                                        🗑 Eliminar
                                                    </button>
                                                </div>
                                            </div>
                                        </div>
                                    )
                                })}
                            </div>
                        )}
                    </div>
                )}

                {/* MÉTRICAS */}
                {tab === 'metricas' && (
                    <div>
                        <div style={styles.filterRow}>
                            <input
                                style={styles.filterInput}
                                type="date"
                                value={periodo.startDate}
                                onChange={e => setPeriodo({ ...periodo, startDate: e.target.value })}
                            />
                            <span style={{ color: '#666' }}>→</span>
                            <input
                                style={styles.filterInput}
                                type="date"
                                value={periodo.endDate}
                                onChange={e => setPeriodo({ ...periodo, endDate: e.target.value })}
                            />
                            <button style={styles.filterBtn} onClick={fetchMetrics}>Aplicar</button>
                        </div>

                        {loading ? <div style={styles.loading}>Cargando métricas...</div> : metrics && (
                            <div>
                                <div style={styles.metricsGrid}>
                                    <div style={styles.metricCard}>
                                        <p style={styles.metricLabel}>Ingresos Totales</p>
                                        <p style={styles.metricValue}>${metrics.totalRevenue?.toFixed(2)}</p>
                                    </div>
                                    <div style={styles.metricCard}>
                                        <p style={styles.metricLabel}>Reservas Pendientes</p>
                                        <p style={styles.metricValue}>{metrics.reservationsByStatus?.pending || 0}</p>
                                    </div>
                                    <div style={styles.metricCard}>
                                        <p style={styles.metricLabel}>Reservas Confirmadas</p>
                                        <p style={styles.metricValue}>{metrics.reservationsByStatus?.confirmed || 0}</p>
                                    </div>
                                    <div style={styles.metricCard}>
                                        <p style={styles.metricLabel}>Reservas Completadas</p>
                                        <p style={styles.metricValue}>{metrics.reservationsByStatus?.completed || 0}</p>
                                    </div>
                                </div>

                                {metrics.topProperty && (
                                    <div style={styles.topCard}>
                                        <p style={styles.topLabel}>🏆 Inmueble más rentable</p>
                                        <p style={styles.topTitle}>{metrics.topProperty.title}</p>
                                        <p style={styles.topRevenue}>${metrics.topProperty.totalRevenue?.toFixed(2)}</p>
                                    </div>
                                )}

                                {metrics.occupancyByProperty?.length > 0 && (
                                    <div style={styles.occupancySection}>
                                        <h3 style={styles.sectionTitle}>Tasa de ocupación por inmueble</h3>
                                        {metrics.occupancyByProperty.map(item => (
                                            <div key={item.inmuebleId} style={styles.occupancyRow}>
                                                <span style={styles.occupancyName}>{item.title}</span>
                                                <div style={styles.progressBar}>
                                                    <div style={{ ...styles.progressFill, width: `${item.occupancyRate}%` }} />
                                                </div>
                                                <span style={styles.occupancyRate}>{item.occupancyRate?.toFixed(1)}%</span>
                                            </div>
                                        ))}
                                    </div>
                                )}
                            </div>
                        )}
                    </div>
                )}

                {/* REPORTES */}
                {tab === 'reportes' && (
                    <div style={styles.reportSection}>
                        <h3 style={styles.sectionTitle}>Descargar reporte en Excel</h3>
                        <p style={styles.reportDesc}>
                            Descarga un reporte con todas tus reservas confirmadas y completadas.
                            Puedes filtrar por período o por inmueble específico.
                        </p>

                        <div style={styles.filterCol}>
                            <div style={styles.filterRow}>
                                <div style={styles.filterField}>
                                    <label style={styles.filterLabel}>Inmueble</label>
                                    <select
                                        style={styles.filterSelect}
                                        value={inmuebleSeleccionado}
                                        onChange={e => setInmuebleSeleccionado(e.target.value)}
                                    >
                                        <option value="">Todos los inmuebles</option>
                                        {inmuebles.map(i => (
                                            <option key={i.id} value={i.id}>{i.title}</option>
                                        ))}
                                    </select>
                                </div>
                            </div>

                            <div style={styles.filterRow}>
                                <div style={styles.filterField}>
                                    <label style={styles.filterLabel}>Desde</label>
                                    <input
                                        style={styles.filterInput}
                                        type="date"
                                        value={periodo.startDate}
                                        onChange={e => setPeriodo({ ...periodo, startDate: e.target.value })}
                                    />
                                </div>
                                <span style={{ color: '#666', marginTop: '24px' }}>→</span>
                                <div style={styles.filterField}>
                                    <label style={styles.filterLabel}>Hasta</label>
                                    <input
                                        style={styles.filterInput}
                                        type="date"
                                        value={periodo.endDate}
                                        onChange={e => setPeriodo({ ...periodo, endDate: e.target.value })}
                                    />
                                </div>
                            </div>
                        </div>

                        <button
                            style={{ ...styles.downloadBtn, opacity: descargando ? 0.7 : 1 }}
                            onClick={handleDescargarReporte}
                            disabled={descargando}
                        >
                            {descargando ? 'Generando...' : '📥 Descargar Excel'}
                        </button>
                    </div>
                )}
            </div>
        </div>
    )
}

const styles = {
    filterCol: { display: 'flex', flexDirection: 'column', gap: '16px', marginBottom: '24px' },
    filterField: { display: 'flex', flexDirection: 'column', gap: '6px' },
    filterLabel: { fontSize: '13px', fontWeight: '500', color: '#555' },
    filterSelect: {
        padding: '10px 14px', borderRadius: '8px', border: '1px solid #ddd',
        fontSize: '14px', outline: 'none', backgroundColor: 'white',
    },
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
    container: { maxWidth: '1100px', margin: '0 auto', padding: '40px 24px' },
    pageTitle: { fontSize: '28px', fontWeight: '700', marginBottom: '4px' },
    pageSubtitle: { color: '#666', marginBottom: '32px' },
    tabs: { display: 'flex', gap: '4px', borderBottom: '2px solid #eee', marginBottom: '32px' },
    tab: {
        padding: '12px 20px', border: 'none', backgroundColor: 'transparent',
        cursor: 'pointer', fontSize: '15px', color: '#666', fontWeight: '500',
    },
    tabActive: { color: '#2D6A4F', borderBottom: '2px solid #2D6A4F', marginBottom: '-2px' },
    addBtn: {
        backgroundColor: '#2D6A4F', color: 'white', border: 'none',
        padding: '10px 20px', borderRadius: '8px', cursor: 'pointer',
        fontSize: '14px', fontWeight: '600', marginBottom: '24px',
    },
    loading: { textAlign: 'center', padding: '60px', color: '#666' },
    grid: { display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: '20px' },
    card: { backgroundColor: 'white', borderRadius: '12px', overflow: 'hidden', boxShadow: '0 2px 8px rgba(0,0,0,0.06)' },
    cardImg: { height: '180px', position: 'relative', backgroundColor: '#f0f0f0' },
    img: { width: '100%', height: '100%', objectFit: 'cover' },
    imgPlaceholder: { height: '100%', display: 'flex', alignItems: 'center', justifyContent: 'center', fontSize: '48px' },
    badge: {
        position: 'absolute', top: '12px', right: '12px',
        padding: '4px 10px', borderRadius: '20px', fontSize: '12px', fontWeight: '600',
    },
    cardBody: { padding: '16px' },
    cardTitle: { fontSize: '16px', fontWeight: '600', marginBottom: '4px' },
    cardLocation: { fontSize: '13px', color: '#888', marginBottom: '6px' },
    cardPrice: { fontSize: '14px', color: '#2D6A4F', fontWeight: '600', marginBottom: '12px' },
    cardActions: { display: 'flex', gap: '8px' },
    editBtn: {
        flex: 1, backgroundColor: 'transparent', border: '1.5px solid #ddd',
        padding: '8px', borderRadius: '8px', cursor: 'pointer', fontSize: '13px',
    },
    deleteBtn: {
        flex: 1, backgroundColor: 'transparent', border: '1.5px solid #ffd0d0',
        color: '#e74c3c', padding: '8px', borderRadius: '8px', cursor: 'pointer', fontSize: '13px',
    },
    filterRow: { display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '24px' },
    filterInput: {
        padding: '10px 14px', borderRadius: '8px', border: '1px solid #ddd',
        fontSize: '14px', outline: 'none',
    },
    filterBtn: {
        backgroundColor: '#2D6A4F', color: 'white', border: 'none',
        padding: '10px 20px', borderRadius: '8px', cursor: 'pointer', fontSize: '14px',
    },
    metricsGrid: { display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(200px, 1fr))', gap: '16px', marginBottom: '24px' },
    metricCard: {
        backgroundColor: 'white', borderRadius: '12px', padding: '20px',
        boxShadow: '0 2px 8px rgba(0,0,0,0.06)', textAlign: 'center',
    },
    metricLabel: { fontSize: '13px', color: '#888', marginBottom: '8px' },
    metricValue: { fontSize: '28px', fontWeight: '700', color: '#2D6A4F' },
    topCard: {
        backgroundColor: '#f0fff4', borderRadius: '12px', padding: '20px',
        border: '1px solid #c8e6c9', marginBottom: '24px',
    },
    topLabel: { fontSize: '13px', color: '#2D6A4F', fontWeight: '600', marginBottom: '4px' },
    topTitle: { fontSize: '18px', fontWeight: '700', marginBottom: '4px' },
    topRevenue: { fontSize: '24px', fontWeight: '700', color: '#2D6A4F' },
    occupancySection: { backgroundColor: 'white', borderRadius: '12px', padding: '24px', boxShadow: '0 2px 8px rgba(0,0,0,0.06)' },
    sectionTitle: { fontSize: '18px', fontWeight: '600', marginBottom: '20px' },
    occupancyRow: { display: 'flex', alignItems: 'center', gap: '12px', marginBottom: '16px' },
    occupancyName: { width: '160px', fontSize: '14px', color: '#333' },
    progressBar: { flex: 1, height: '8px', backgroundColor: '#eee', borderRadius: '4px', overflow: 'hidden' },
    progressFill: { height: '100%', backgroundColor: '#2D6A4F', borderRadius: '4px' },
    occupancyRate: { width: '48px', fontSize: '14px', fontWeight: '600', color: '#2D6A4F', textAlign: 'right' },
    reportSection: { backgroundColor: 'white', borderRadius: '12px', padding: '32px', boxShadow: '0 2px 8px rgba(0,0,0,0.06)', maxWidth: '600px' },
    reportDesc: { color: '#666', marginBottom: '24px', lineHeight: '1.6' },
    downloadBtn: {
        backgroundColor: '#2D6A4F', color: 'white', border: 'none',
        padding: '12px 28px', borderRadius: '8px', cursor: 'pointer',
        fontSize: '15px', fontWeight: '600', marginTop: '8px',
    },
}