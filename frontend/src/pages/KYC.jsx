import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { validateKYC } from '../services/api'
import { useAuth } from '../context/AuthContext'

export default function KYC() {
    const { user, loginUser } = useAuth()
    const navigate = useNavigate()
    const [step, setStep] = useState(1) // 1=subir, 2=procesando, 3=resultado
    const [archivo, setArchivo] = useState(null)
    const [preview, setPreview] = useState(null)
    const [resultado, setResultado] = useState(null)
    const [error, setError] = useState('')

    const handleArchivo = (e) => {
        const file = e.target.files[0]
        if (!file) return
        setArchivo(file)
        setPreview(URL.createObjectURL(file))
        setError('')
    }

    const handleDrop = (e) => {
        e.preventDefault()
        const file = e.dataTransfer.files[0]
        if (!file) return
        setArchivo(file)
        setPreview(URL.createObjectURL(file))
        setError('')
    }

    const handleValidar = async () => {
        if (!archivo) {
            setError('Selecciona una imagen de tu documento de identidad')
            return
        }
        setStep(2)
        try {
            const formData = new FormData()
            formData.append('image', archivo)
            const res = await validateKYC(formData)
            setResultado(res.data.data)
            setStep(3)
        } catch (err) {
            setError(err.response?.data?.message || 'Error al procesar el documento')
            setStep(1)
        }
    }

    const stepLabels = ['1. Identidad', '2. Procesando', '3. Verificado']

    return (
        <div style={styles.overlay}>
            <div style={styles.modal}>
                {/* Stepper */}
                <div style={styles.stepper}>
                    {stepLabels.map((label, i) => (
                        <div key={i} style={styles.stepItem}>
              <span style={{
                  ...styles.stepLabel,
                  color: step === i + 1 ? '#2D6A4F' : '#aaa',
                  fontWeight: step === i + 1 ? '700' : '400',
              }}>
                {label}
              </span>
                            {i < stepLabels.length - 1 && <div style={styles.stepLine} />}
                        </div>
                    ))}
                </div>

                {/* Paso 1 — Subir documento */}
                {step === 1 && (
                    <div style={styles.content}>
                        <h2 style={styles.title}>Verifica tu identidad</h2>
                        <p style={styles.subtitle}>
                            Para garantizar la seguridad de nuestra comunidad, necesitamos
                            validar tu documento oficial antes de tu primera reserva.
                        </p>

                        <div style={styles.badges}>
                            <div style={styles.badge}>🔒 Datos encriptados</div>
                            <div style={styles.badge}>🛡 Comunidad segura</div>
                            <div style={styles.badge}>⚡ Validación rápida</div>
                        </div>

                        {!preview ? (
                            <div
                                style={styles.dropzone}
                                onDrop={handleDrop}
                                onDragOver={(e) => e.preventDefault()}
                                onClick={() => document.getElementById('kyc-input').click()}
                            >
                                <input
                                    id="kyc-input"
                                    type="file"
                                    accept="image/*"
                                    style={{ display: 'none' }}
                                    onChange={handleArchivo}
                                />
                                <span style={styles.dropzoneIcon}>📷</span>
                                <p style={styles.dropzoneText}>Sube tu DNI o Pasaporte</p>
                                <p style={styles.dropzoneHint}>Arrastra aquí o haz clic para abrir la cámara</p>
                            </div>
                        ) : (
                            <div style={styles.previewContainer}>
                                <img src={preview} alt="Documento" style={styles.previewImg} />
                                <button
                                    style={styles.changeBtn}
                                    onClick={() => { setArchivo(null); setPreview(null) }}
                                >
                                    Cambiar imagen
                                </button>
                            </div>
                        )}

                        {error && <div style={styles.error}>⚠️ {error}</div>}

                        <button
                            style={{ ...styles.validateBtn, opacity: !archivo ? 0.6 : 1 }}
                            onClick={handleValidar}
                            disabled={!archivo}
                        >
                            Validar documento →
                        </button>

                        <button style={styles.cancelBtn} onClick={() => navigate(-1)}>
                            Cancelar
                        </button>
                    </div>
                )}

                {/* Paso 2 — Procesando */}
                {step === 2 && (
                    <div style={styles.content}>
                        <div style={styles.processingIcon}>⏳</div>
                        <h2 style={styles.title}>Procesando tu documento</h2>
                        <p style={styles.subtitle}>
                            Nuestra IA está analizando tu documento de identidad.
                            Esto puede tomar unos segundos...
                        </p>
                        <div style={styles.spinner} />
                    </div>
                )}

                {/* Paso 3 — Resultado */}
                {step === 3 && resultado && (
                    <div style={styles.content}>
                        <div style={styles.resultIcon}>
                            {resultado.verdict === 'approved' ? '✅' : '❌'}
                        </div>
                        <h2 style={{
                            ...styles.title,
                            color: resultado.verdict === 'approved' ? '#27ae60' : '#e74c3c'
                        }}>
                            {resultado.verdict === 'approved' ? '¡Identidad verificada!' : 'Verificación rechazada'}
                        </h2>
                        <p style={styles.subtitle}>
                            {resultado.verdict === 'approved'
                                ? 'Tu identidad ha sido validada exitosamente. Ya puedes realizar reservas.'
                                : 'No pudimos validar tu documento. Asegúrate de que la foto sea clara y legible.'
                            }
                        </p>

                        {resultado.verdict === 'approved' && (
                            <div style={styles.dataCard}>
                                <h3 style={styles.dataTitle}>Datos extraídos</h3>
                                <div style={styles.dataRow}>
                                    <span style={styles.dataLabel}>Nombre</span>
                                    <span style={styles.dataValue}>{resultado.extractedName} {resultado.extractedLastname}</span>
                                </div>
                                <div style={styles.dataRow}>
                                    <span style={styles.dataLabel}>Documento</span>
                                    <span style={styles.dataValue}>{resultado.extractedDocumentNumber}</span>
                                </div>
                                {resultado.extractedBirthdate && (
                                    <div style={styles.dataRow}>
                                        <span style={styles.dataLabel}>Fecha de nacimiento</span>
                                        <span style={styles.dataValue}>{resultado.extractedBirthdate}</span>
                                    </div>
                                )}
                            </div>
                        )}

                        <button
                            style={styles.validateBtn}
                            onClick={() => navigate(resultado.verdict === 'approved' ? '/dashboard-guest' : '/kyc')}
                        >
                            {resultado.verdict === 'approved' ? 'Continuar' : 'Intentar de nuevo'}
                        </button>
                    </div>
                )}

                <div style={styles.footer}>
                    🔒 Seguridad de nivel bancario por RentasCortas Verified
                </div>
            </div>
        </div>
    )
}

const styles = {
    overlay: {
        minHeight: '100vh', backgroundColor: 'rgba(0,0,0,0.4)',
        display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '24px',
    },
    modal: {
        backgroundColor: 'white', borderRadius: '20px', width: '100%',
        maxWidth: '560px', overflow: 'hidden',
        boxShadow: '0 20px 60px rgba(0,0,0,0.2)',
    },
    stepper: {
        display: 'flex', alignItems: 'center', padding: '20px 32px',
        borderBottom: '1px solid #eee',
    },
    stepItem: { display: 'flex', alignItems: 'center', flex: 1 },
    stepLabel: { fontSize: '13px', whiteSpace: 'nowrap' },
    stepLine: { flex: 1, height: '1px', backgroundColor: '#eee', margin: '0 8px' },
    content: {
        padding: '32px', display: 'flex', flexDirection: 'column',
        alignItems: 'center', textAlign: 'center',
    },
    title: { fontSize: '22px', fontWeight: '700', marginBottom: '8px', color: '#1a1a1a' },
    subtitle: { color: '#666', fontSize: '14px', lineHeight: '1.6', marginBottom: '24px', maxWidth: '400px' },
    badges: { display: 'flex', gap: '8px', marginBottom: '24px', flexWrap: 'wrap', justifyContent: 'center' },
    badge: {
        backgroundColor: '#f5f5f5', padding: '8px 14px', borderRadius: '8px',
        fontSize: '13px', color: '#555',
    },
    dropzone: {
        width: '100%', border: '2px dashed #ddd', borderRadius: '12px',
        padding: '40px 20px', cursor: 'pointer', marginBottom: '16px',
        backgroundColor: '#fafafa', display: 'flex', flexDirection: 'column',
        alignItems: 'center', gap: '8px',
    },
    dropzoneIcon: { fontSize: '40px' },
    dropzoneText: { fontSize: '16px', fontWeight: '600', color: '#333' },
    dropzoneHint: { fontSize: '13px', color: '#888' },
    previewContainer: { width: '100%', marginBottom: '16px' },
    previewImg: { width: '100%', maxHeight: '200px', objectFit: 'cover', borderRadius: '12px', marginBottom: '8px' },
    changeBtn: {
        backgroundColor: 'transparent', border: '1px solid #ddd', padding: '6px 14px',
        borderRadius: '8px', cursor: 'pointer', fontSize: '13px', color: '#666',
    },
    error: {
        backgroundColor: '#fff0f0', color: '#c0392b', padding: '12px 16px',
        borderRadius: '8px', fontSize: '14px', width: '100%',
        textAlign: 'left', marginBottom: '16px',
    },
    validateBtn: {
        width: '100%', backgroundColor: '#2D6A4F', color: 'white', border: 'none',
        padding: '14px', borderRadius: '8px', fontSize: '16px', fontWeight: '600',
        cursor: 'pointer', marginBottom: '8px',
    },
    cancelBtn: {
        backgroundColor: 'transparent', border: 'none', color: '#888',
        padding: '10px', cursor: 'pointer', fontSize: '14px',
    },
    processingIcon: { fontSize: '64px', marginBottom: '16px' },
    spinner: {
        width: '40px', height: '40px', border: '4px solid #eee',
        borderTop: '4px solid #2D6A4F', borderRadius: '50%',
        animation: 'spin 1s linear infinite', marginTop: '24px',
    },
    resultIcon: { fontSize: '72px', marginBottom: '16px' },
    dataCard: {
        width: '100%', backgroundColor: '#f8f9fa', borderRadius: '12px',
        padding: '20px', marginBottom: '20px', textAlign: 'left',
    },
    dataTitle: { fontSize: '14px', fontWeight: '600', color: '#555', marginBottom: '12px' },
    dataRow: { display: 'flex', justifyContent: 'space-between', marginBottom: '8px' },
    dataLabel: { fontSize: '13px', color: '#888' },
    dataValue: { fontSize: '13px', fontWeight: '600', color: '#333' },
    footer: {
        padding: '14px', textAlign: 'center', fontSize: '12px',
        color: '#aaa', borderTop: '1px solid #eee', backgroundColor: '#fafafa',
    },
}