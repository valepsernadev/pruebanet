import { useState, useEffect } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { createInmueble, updateInmueble, getInmueble, addImage } from '../services/api'
import { useAuth } from '../context/AuthContext'

export default function InmuebleForm() {
    const { id } = useParams()
    const isEditing = !!id
    const { user } = useAuth()
    const navigate = useNavigate()
    
    const [form, setForm] = useState({
        title: '', description: '', location: '', pricePerNight: '', status: 'active'
    })
    const [imagenes, setImagenes] = useState([])
    const [previews, setPreviews] = useState([])
    const [loading, setLoading] = useState(false)
    const [loadingImg, setLoadingImg] = useState(false)
    const [error, setError] = useState('')
    const [success, setSuccess] = useState('')

    useEffect(() => {
        if (!user || user.role !== 'owner') { navigate('/login'); return }
        if (isEditing) fetchInmueble()
    }, [])

    const fetchInmueble = async () => {
        try {
            const res = await getInmueble(id)
            const data = res.data.data
            setForm({
                title: data.title,
                description: data.description || '',
                location: data.location,
                pricePerNight: data.pricePerNight,
                status: data.status || 'active',
            })
        } catch {
            navigate('/dashboard-owner')
        }
    }
    

    const handleChange = (e) => {
        setForm({ ...form, [e.target.name]: e.target.value })
        setError('')
    }

    const handleImageChange = (e) => {
        const files = Array.from(e.target.files)
        setImagenes(files)
        setPreviews(files.map(f => URL.createObjectURL(f)))
    }

    const handleSubmit = async (e) => {
        e.preventDefault()
        console.log('Form que se envía:', form)
        if (!form.title || !form.location || !form.pricePerNight) {
            setError('Completa todos los campos obligatorios')
            return
        }
        setLoading(true)
        try {
            let inmuebleId = id
            if (isEditing) {
                await updateInmueble(id, { ...form, status: form.status || 'active' })
            } else {
                const res = await createInmueble(form)
                inmuebleId = res.data.data.id
            }

            // Subir imágenes si hay
            if (imagenes.length > 0) {
                setLoadingImg(true)
                for (const imagen of imagenes) {
                    const formData = new FormData()
                    formData.append('file', imagen)
                    await addImage(inmuebleId, formData)
                }
                setLoadingImg(false)
            }

            setSuccess(isEditing ? 'Inmueble actualizado exitosamente' : 'Inmueble publicado exitosamente')
            setTimeout(() => navigate('/dashboard-owner'), 1500)
        } catch (err) {
            setError(err.response?.data?.message || 'Error al guardar el inmueble')
        } finally {
            setLoading(false)
        }
    }

    return (
        <div style={styles.page}>
            <header style={styles.header}>
                <h1 style={styles.logo} onClick={() => navigate('/')}>RentasCortas</h1>
                <button style={styles.backBtn} onClick={() => navigate('/dashboard-owner')}>
                    ← Volver al panel
                </button>
            </header>

            <div style={styles.container}>
                <h2 style={styles.title}>
                    {isEditing ? 'Editar propiedad' : 'Detalles de tu propiedad'}
                </h2>
                <p style={styles.subtitle}>
                    {isEditing ? 'Actualiza la información de tu inmueble' : 'Cuéntanos sobre tu espacio para que los viajeros puedan encontrarlo fácilmente.'}
                </p>

                <form onSubmit={handleSubmit} style={styles.form}>
                    {/* Información básica */}
                    <div style={styles.section}>
                        <h3 style={styles.sectionTitle}>📋 Información Básica</h3>
                        <div style={styles.field}>
                            <label style={styles.label}>Título del anuncio *</label>
                            <input
                                style={styles.input}
                                name="title"
                                placeholder="Ej. Villa Moderna con Piscina Privada"
                                value={form.title}
                                onChange={handleChange}
                                required
                            />
                        </div>
                        <div style={styles.field}>
                            <label style={styles.label}>Descripción</label>
                            <textarea
                                style={styles.textarea}
                                name="description"
                                placeholder="Describe qué hace único a tu espacio, las comodidades y el vecindario..."
                                value={form.description}
                                onChange={handleChange}
                                rows={5}
                            />
                        </div>
                        {isEditing && (
                            <div style={styles.field}>
                                <label style={styles.label}>Estado del inmueble</label>
                                <select
                                    style={styles.input}
                                    name="status"
                                    value={form.status}
                                    onChange={handleChange}
                                >
                                    <option value="active">Activo — visible para huéspedes</option>
                                    <option value="inactive">Inactivo — oculto temporalmente</option>
                                </select>
                            </div>
                        )}
                    </div>

                    {/* Ubicación y precio */}
                    <div style={styles.section}>
                        <div style={styles.twoCol}>
                            <div>
                                <h3 style={styles.sectionTitle}>📍 Ubicación</h3>
                                <div style={styles.field}>
                                    <label style={styles.label}>Ciudad o dirección *</label>
                                    <input
                                        style={styles.input}
                                        name="location"
                                        placeholder="Ciudad, País o Dirección"
                                        value={form.location}
                                        onChange={handleChange}
                                        required
                                    />
                                </div>
                            </div>
                            <div>
                                <h3 style={styles.sectionTitle}>💰 Precio</h3>
                                <div style={styles.field}>
                                    <label style={styles.label}>Precio por noche (USD) *</label>
                                    <div style={styles.priceWrapper}>
                                        <span style={styles.priceDollar}>$</span>
                                        <input
                                            style={styles.priceInput}
                                            name="pricePerNight"
                                            type="number"
                                            min="1"
                                            placeholder="0.00"
                                            value={form.pricePerNight}
                                            onChange={handleChange}
                                            required
                                        />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>

                    {/* Fotografías */}
                    <div style={styles.section}>
                        <h3 style={styles.sectionTitle}>📸 Fotografías</h3>
                        <p style={styles.hint}>Mínimo 1 foto recomendada. PNG, JPG o WEBP (Máx. 10MB)</p>

                        <label style={styles.dropzone}>
                            <input
                                type="file"
                                accept="image/*"
                                multiple
                                style={{ display: 'none' }}
                                onChange={handleImageChange}
                            />
                            <div style={styles.dropzoneContent}>
                                <span style={styles.dropzoneIcon}>📁</span>
                                <p style={styles.dropzoneText}>Haz clic para subir o arrastra y suelta</p>
                                <p style={styles.dropzoneHint}>PNG, JPG o WEBP (Máx. 10MB)</p>
                            </div>
                        </label>

                        {previews.length > 0 && (
                            <div style={styles.previews}>
                                {previews.map((src, i) => (
                                    <div key={i} style={styles.previewWrapper}>
                                        {i === 0 && <span style={styles.portadaBadge}>PORTADA</span>}
                                        <img src={src} alt="" style={styles.previewImg} />
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>

                    {error && <div style={styles.error}>⚠️ {error}</div>}
                    {success && <div style={styles.successMsg}>✅ {success}</div>}

                    <div style={styles.actions}>
                        <button
                            type="button"
                            style={styles.cancelBtn}
                            onClick={() => navigate('/dashboard-owner')}
                        >
                            Descartar
                        </button>
                        <button
                            type="submit"
                            style={{ ...styles.submitBtn, opacity: loading ? 0.7 : 1 }}
                            disabled={loading}
                        >
                            {loading
                                ? loadingImg ? 'Subiendo imágenes...' : 'Guardando...'
                                : isEditing ? 'Guardar cambios' : 'Publicar inmueble'
                            }
                        </button>
                    </div>
                </form>
            </div>
        </div>
    )
}

const styles = {
    page: { minHeight: '100vh', backgroundColor: '#f8f9fa' },
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
    container: { maxWidth: '800px', margin: '0 auto', padding: '40px 24px' },
    title: { fontSize: '28px', fontWeight: '700', marginBottom: '8px' },
    subtitle: { color: '#666', marginBottom: '32px' },
    form: { display: 'flex', flexDirection: 'column', gap: '24px' },
    section: {
        backgroundColor: 'white', borderRadius: '12px', padding: '28px',
        boxShadow: '0 2px 8px rgba(0,0,0,0.06)',
    },
    sectionTitle: { fontSize: '16px', fontWeight: '600', marginBottom: '20px', color: '#333' },
    twoCol: { display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '32px' },
    field: { display: 'flex', flexDirection: 'column', gap: '6px', marginBottom: '16px' },
    label: { fontSize: '14px', fontWeight: '500', color: '#333' },
    input: {
        padding: '12px 16px', borderRadius: '8px', border: '1px solid #ddd',
        fontSize: '15px', outline: 'none',
    },
    textarea: {
        padding: '12px 16px', borderRadius: '8px', border: '1px solid #ddd',
        fontSize: '15px', outline: 'none', resize: 'vertical', fontFamily: 'inherit',
    },
    priceWrapper: { display: 'flex', alignItems: 'center', border: '1px solid #ddd', borderRadius: '8px', overflow: 'hidden' },
    priceDollar: { padding: '12px 14px', backgroundColor: '#f5f5f5', color: '#555', fontSize: '15px' },
    priceInput: { flex: 1, padding: '12px', border: 'none', outline: 'none', fontSize: '15px' },
    hint: { fontSize: '13px', color: '#888', marginBottom: '16px' },
    dropzone: {
        display: 'block', border: '2px dashed #ddd', borderRadius: '12px',
        padding: '40px', textAlign: 'center', cursor: 'pointer',
        backgroundColor: '#fafafa', marginBottom: '16px',
    },
    dropzoneContent: { display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '8px' },
    dropzoneIcon: { fontSize: '32px' },
    dropzoneText: { fontSize: '15px', fontWeight: '500', color: '#333' },
    dropzoneHint: { fontSize: '13px', color: '#888' },
    previews: { display: 'flex', gap: '12px', flexWrap: 'wrap' },
    previewWrapper: { position: 'relative', width: '100px', height: '80px' },
    portadaBadge: {
        position: 'absolute', top: '4px', left: '4px', zIndex: 1,
        backgroundColor: '#2D6A4F', color: 'white', fontSize: '9px',
        fontWeight: '700', padding: '2px 6px', borderRadius: '4px',
    },
    previewImg: { width: '100%', height: '100%', objectFit: 'cover', borderRadius: '8px' },
    error: {
        backgroundColor: '#fff0f0', color: '#c0392b', padding: '14px 16px',
        borderRadius: '8px', fontSize: '14px', border: '1px solid #ffd0d0',
    },
    successMsg: {
        backgroundColor: '#f0fff4', color: '#27ae60', padding: '14px 16px',
        borderRadius: '8px', fontSize: '14px', border: '1px solid #c8e6c9',
    },
    actions: { display: 'flex', justifyContent: 'space-between', alignItems: 'center', paddingTop: '8px' },
    cancelBtn: {
        backgroundColor: 'transparent', border: 'none', color: '#888',
        padding: '12px 20px', cursor: 'pointer', fontSize: '14px',
    },
    submitBtn: {
        backgroundColor: '#2D6A4F', color: 'white', border: 'none',
        padding: '14px 32px', borderRadius: '8px', cursor: 'pointer',
        fontSize: '16px', fontWeight: '600',
    },
}