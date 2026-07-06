import { useState } from 'react';
import { useOrganization } from '@clerk/clerk-react';
import { UserPlus, Trash2 } from 'lucide-react';
import TopBar from '../components/layout/TopBar';
import {
  Avatar,
  Button,
  Card,
  Field,
  Input,
  Modal,
  Select,
  useToast,
} from '../components/ui';
import styles from './Team.module.css';

const ROLE_OPTIONS = [
  { value: 'org:member', label: 'Miembro' },
  { value: 'org:admin', label: 'Admin' },
];

function roleLabel(role: string): string {
  if (role === 'org:admin') return 'Admin';
  if (role === 'org:member') return 'Miembro';
  return role;
}

export default function Team() {
  const { addToast } = useToast();

  const { organization, membership, memberships, invitations } = useOrganization({
    memberships: { infinite: true, keepPreviousData: true },
    invitations: { infinite: true, keepPreviousData: true },
  });

  const isAdmin = membership?.role === 'org:admin';

  // Estado del modal de invitación
  const [inviteOpen, setInviteOpen] = useState(false);
  const [inviteEmail, setInviteEmail] = useState('');
  const [inviteRole, setInviteRole] = useState('org:member');
  const [inviting, setInviting] = useState(false);

  async function handleInvite() {
    if (!organization || !inviteEmail.trim()) return;
    setInviting(true);
    try {
      await organization.inviteMember({
        emailAddress: inviteEmail.trim(),
        role: inviteRole,
      });
      addToast('Invitación enviada correctamente.', 'success');
      setInviteEmail('');
      setInviteRole('org:member');
      setInviteOpen(false);
      await invitations?.revalidate?.();
    } catch {
      addToast('No se pudo enviar la invitación.', 'error');
    } finally {
      setInviting(false);
    }
  }

  async function handleChangeRole(membershipId: string, newRole: string) {
    const m = memberships?.data?.find((x) => x.id === membershipId);
    if (!m) return;
    try {
      await m.update({ role: newRole });
      addToast('Rol actualizado.', 'success');
      await memberships?.revalidate?.();
    } catch {
      addToast('No se pudo cambiar el rol.', 'error');
    }
  }

  async function handleRemoveMember(membershipId: string) {
    const m = memberships?.data?.find((x) => x.id === membershipId);
    if (!m) return;
    try {
      await m.destroy();
      addToast('Miembro eliminado.', 'success');
      await memberships?.revalidate?.();
    } catch {
      addToast('No se pudo eliminar al miembro.', 'error');
    }
  }

  async function handleRevokeInvitation(invitationId: string) {
    const inv = invitations?.data?.find((x) => x.id === invitationId);
    if (!inv) return;
    try {
      await inv.revoke();
      addToast('Invitación revocada.', 'success');
      await invitations?.revalidate?.();
    } catch {
      addToast('No se pudo revocar la invitación.', 'error');
    }
  }

  const loading = memberships?.isLoading || invitations?.isLoading;

  return (
    <>
      <TopBar
        title="Equipo"
        subtitle="Invita a tu equipo y gestiona sus permisos."
      />

      <div className={styles.content}>
        <Card>
          <div className={styles.cardHeader}>
            <div>
              <h2 className={styles.cardTitle}>Miembros</h2>
              <p className={styles.cardSubtitle}>
                {memberships?.data?.length ?? 0} activos
                {invitations?.data?.length
                  ? ` · ${invitations.data.length} pendientes`
                  : ''}
              </p>
            </div>
            {isAdmin && (
              <Button variant="blue" onClick={() => setInviteOpen(true)}>
                <UserPlus size={16} strokeWidth={1.8} />
                Invitar miembro
              </Button>
            )}
          </div>

          {loading ? (
            <p className={styles.loading}>Cargando equipo…</p>
          ) : (
            <div className={styles.table}>
              {memberships?.data?.map((m) => (
                <div key={m.id} className={styles.row}>
                  <div className={styles.user}>
                    <Avatar
                      name={
                        m.publicUserData?.firstName
                          ? `${m.publicUserData.firstName} ${m.publicUserData.lastName ?? ''}`
                          : m.publicUserData?.identifier ?? '?'
                      }
                      src={m.publicUserData?.imageUrl}
                    />
                    <div className={styles.userInfo}>
                      <span className={styles.userName}>
                        {m.publicUserData?.firstName
                          ? `${m.publicUserData.firstName} ${m.publicUserData.lastName ?? ''}`
                          : m.publicUserData?.identifier}
                      </span>
                      <span className={styles.userEmail}>
                        {m.publicUserData?.identifier}
                      </span>
                    </div>
                  </div>

                  <span className={`${styles.badge} ${styles.badgeActive}`}>
                    Activo
                  </span>

                  {isAdmin && m.publicUserData?.userId !== membership?.publicUserData?.userId ? (
                    <Select
                      options={ROLE_OPTIONS}
                      value={m.role}
                      onChange={(v) => handleChangeRole(m.id, v)}
                    />
                  ) : (
                    <span className={styles.roleStatic}>{roleLabel(m.role)}</span>
                  )}

                  {isAdmin && m.publicUserData?.userId !== membership?.publicUserData?.userId ? (
                    <button
                      type="button"
                      className={styles.iconBtn}
                      onClick={() => handleRemoveMember(m.id)}
                      aria-label="Eliminar miembro"
                    >
                      <Trash2 size={16} strokeWidth={1.6} />
                    </button>
                  ) : (
                    <span />
                  )}
                </div>
              ))}

              {invitations?.data?.map((inv) => (
                <div key={inv.id} className={styles.row}>
                  <div className={styles.user}>
                    <Avatar name={inv.emailAddress} />
                    <div className={styles.userInfo}>
                      <span className={styles.userName}>{inv.emailAddress}</span>
                      <span className={styles.userEmail}>Invitación pendiente</span>
                    </div>
                  </div>

                  <span className={`${styles.badge} ${styles.badgePending}`}>
                    Pendiente
                  </span>

                  <span className={styles.roleStatic}>{roleLabel(inv.role)}</span>

                  {isAdmin ? (
                    <button
                      type="button"
                      className={styles.iconBtn}
                      onClick={() => handleRevokeInvitation(inv.id)}
                      aria-label="Revocar invitación"
                    >
                      <Trash2 size={16} strokeWidth={1.6} />
                    </button>
                  ) : (
                    <span />
                  )}
                </div>
              ))}

              {!memberships?.data?.length && !invitations?.data?.length && (
                <p className={styles.empty}>Aún no hay miembros en el equipo.</p>
              )}
            </div>
          )}
        </Card>
      </div>

      <Modal
        open={inviteOpen}
        onClose={() => setInviteOpen(false)}
        title="Invitar a un miembro"
        footer={
          <>
            <Button
              variant="secondary"
              size="sm"
              onClick={() => setInviteOpen(false)}
            >
              Cancelar
            </Button>
            <Button
              variant="blue"
              size="sm"
              loading={inviting}
              onClick={handleInvite}
            >
              Enviar invitación
            </Button>
          </>
        }
      >
        <div style={{ display: 'flex', flexDirection: 'column', gap: 16 }}>
          <Field label="Email">
            <Input
              type="email"
              value={inviteEmail}
              onChange={(e) => setInviteEmail(e.target.value)}
              placeholder="persona@empresa.com"
            />
          </Field>
          <Field label="Rol">
            <Select
              options={ROLE_OPTIONS}
              value={inviteRole}
              onChange={setInviteRole}
            />
          </Field>
        </div>
      </Modal>
    </>
  );
}