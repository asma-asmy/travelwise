import { useState } from 'react'
import { Link, useParams } from 'react-router'
import { ArrowLeft } from 'lucide-react'
import { PageHeader } from '@/components/PageHeader'
import { LoadingState } from '@/components/LoadingState'
import { ErrorState } from '@/components/ErrorState'
import { WorkflowStatusPill } from '@/components/WorkflowStatusPill'
import { AgentExecutionCard } from '@/components/workflow/AgentExecutionCard'
import { ToolCallTable } from '@/components/workflow/ToolCallTable'
import { ValidationPanel } from '@/components/workflow/ValidationPanel'
import { ProposalPanel } from '@/components/workflow/ProposalPanel'
import { RiskBudgetReadinessSummary } from '@/components/workflow/RiskBudgetReadinessSummary'
import { ApprovalActionDialog } from '@/components/workflow/ApprovalActionDialog'
import { useApproveWorkflow, useRejectWorkflow, useRequestRevision, useWorkflow } from '@/hooks/useWorkflows'
import { useToastStore } from '@/stores/toastStore'
import { formatDateTime } from '@/utils/format'
import { humanizeStatus } from '@/utils/workflowStatus'

type Tab = 'overview' | 'agents' | 'toolCalls' | 'validation' | 'audit'

export function WorkflowDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data, isLoading, isError, refetch } = useWorkflow(id)

  const [tab, setTab] = useState<Tab>('overview')
  const [dialogAction, setDialogAction] = useState<'APPROVE' | 'REJECT' | 'REVISION_REQUIRED' | null>(null)

  const approve = useApproveWorkflow()
  const reject = useRejectWorkflow()
  const requestRevision = useRequestRevision()
  const pushToast = useToastStore((s) => s.push)

  const submitting = approve.isPending || reject.isPending || requestRevision.isPending

  async function handleConfirm(reason: string) {
    if (!id || !dialogAction) return
    try {
      if (dialogAction === 'APPROVE') {
        await approve.mutateAsync({ id, reason })
        pushToast('success', 'Workflow approved.')
      } else if (dialogAction === 'REJECT') {
        await reject.mutateAsync({ id, reason })
        pushToast('success', 'Workflow rejected.')
      } else {
        await requestRevision.mutateAsync({ id, reason })
        pushToast('success', 'Revision requested.')
      }
      setDialogAction(null)
    } catch {
      pushToast('error', 'Action failed. Please try again.')
    }
  }

  if (isLoading) return <LoadingState />
  if (isError || !data) return <ErrorState message="Failed to load workflow." onRetry={() => refetch()} />

  const canDecide = data.status === 'AwaitingApproval'

  return (
    <>
      <div style={{ marginBottom: '0.5rem' }}>
        <Link to="/workflows" style={{ fontSize: '0.875rem', display: 'inline-flex', alignItems: 'center', gap: '0.25rem' }}>
          <ArrowLeft size={14} />
          Back to workflows
        </Link>
      </div>

      <PageHeader
        title={data.objective}
        subtitle={`Created ${formatDateTime(data.createdAt)}`}
        actions={
          canDecide ? (
            <div style={{ display: 'flex', gap: '0.5rem', flexWrap: 'wrap' }}>
              <button className="btn btn-secondary" onClick={() => setDialogAction('REVISION_REQUIRED')}>
                Request Revision
              </button>
              <button className="btn btn-danger" onClick={() => setDialogAction('REJECT')}>
                Reject
              </button>
              <button className="btn btn-primary" onClick={() => setDialogAction('APPROVE')}>
                Approve
              </button>
            </div>
          ) : null
        }
      />

      <div
        style={{
          display: 'flex',
          gap: '0.75rem',
          alignItems: 'center',
          flexWrap: 'wrap',
          marginBottom: '1.25rem',
        }}
      >
        <WorkflowStatusPill status={data.status} />
        {data.currentStep && (
          <span style={{ fontSize: '0.8125rem', color: 'var(--color-text-muted)' }}>
            Step: <code>{data.currentStep}</code>
          </span>
        )}
        {data.failureReason && (
          <span style={{ fontSize: '0.8125rem', color: 'var(--color-danger)' }}>
            {data.failureReason}
          </span>
        )}
      </div>

      {/* Tabs */}
      <div style={{ display: 'flex', gap: '0.25rem', borderBottom: '1px solid var(--color-border)', marginBottom: '1rem' }}>
        {(
          [
            ['overview', 'Overview'],
            ['agents', `Agents (${data.agentExecutions.length})`],
            ['toolCalls', `Tool Calls (${data.toolCalls.length})`],
            ['validation', `Validation (${data.validations.length})`],
            ['audit', 'Audit & Approvals'],
          ] as [Tab, string][]
        ).map(([key, label]) => (
          <button
            key={key}
            onClick={() => setTab(key)}
            style={{
              padding: '0.5rem 0.875rem',
              background: 'none',
              border: 'none',
              borderBottom: tab === key ? '2px solid var(--color-primary)' : '2px solid transparent',
              color: tab === key ? 'var(--color-primary)' : 'var(--color-text-muted)',
              fontWeight: tab === key ? 600 : 400,
              cursor: 'pointer',
              fontSize: '0.875rem',
            }}
          >
            {label}
          </button>
        ))}
      </div>

      {tab === 'overview' && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
          {/* Plan */}
          {data.plan && (
            <section>
              <h3 style={{ fontSize: '1rem', fontWeight: 600, marginBottom: '0.5rem' }}>Plan</h3>
              <div className="card">
                <ol style={{ margin: 0, paddingLeft: '1.25rem' }}>
                  {data.plan.plan.map((step) => (
                    <li key={step.step_id} style={{ marginBottom: '0.375rem', fontSize: '0.875rem' }}>
                      <strong>{step.agent}</strong> — <code>{step.task}</code>
                      {step.depends_on && step.depends_on.length > 0 && (
                        <span style={{ color: 'var(--color-text-muted)' }}>
                          {' '}
                          (after {step.depends_on.join(', ')})
                        </span>
                      )}
                    </li>
                  ))}
                </ol>
              </div>
            </section>
          )}

          {/* Risk / Budget / Readiness summary */}
          <section>
            <h3 style={{ fontSize: '1rem', fontWeight: 600, marginBottom: '0.5rem' }}>Summary</h3>
            <RiskBudgetReadinessSummary result={data.result} />
          </section>

          {/* Proposal */}
          <section>
            <h3 style={{ fontSize: '1rem', fontWeight: 600, marginBottom: '0.5rem' }}>Proposal</h3>
            <ProposalPanel result={data.result} />
          </section>
        </div>
      )}

      {tab === 'agents' && (
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(300px, 1fr))', gap: '0.75rem' }}>
          {data.agentExecutions.map((ex, i) => (
            <AgentExecutionCard key={ex.id} execution={ex} index={i} />
          ))}
        </div>
      )}

      {tab === 'toolCalls' && <ToolCallTable toolCalls={data.toolCalls} />}

      {tab === 'validation' && <ValidationPanel validations={data.validations} />}

      {tab === 'audit' && (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          <section>
            <h3 style={{ fontSize: '1rem', fontWeight: 600, marginBottom: '0.5rem' }}>Approval decisions</h3>
            {data.approvals.length === 0 ? (
              <div className="card">
                <p style={{ color: 'var(--color-text-muted)' }}>No decisions recorded.</p>
              </div>
            ) : (
              <div className="card" style={{ padding: 0 }}>
                <ul style={{ listStyle: 'none', margin: 0, padding: 0 }}>
                  {data.approvals.map((a, i) => (
                    <li
                      key={a.id}
                      style={{
                        padding: '0.625rem 0.875rem',
                        borderTop: i === 0 ? 'none' : '1px solid var(--color-border)',
                        display: 'flex',
                        justifyContent: 'space-between',
                        gap: '1rem',
                        fontSize: '0.875rem',
                      }}
                    >
                      <div>
                        <strong>{a.decision}</strong>
                        {a.reason ? ` — ${a.reason}` : ''}
                      </div>
                      <div style={{ color: 'var(--color-text-muted)' }}>{formatDateTime(a.decidedAt)}</div>
                    </li>
                  ))}
                </ul>
              </div>
            )}
          </section>

          <section>
            <h3 style={{ fontSize: '1rem', fontWeight: 600, marginBottom: '0.5rem' }}>Status</h3>
            <div className="card">
              <dl style={{ display: 'grid', gridTemplateColumns: 'max-content 1fr', gap: '0.25rem 1rem', fontSize: '0.875rem' }}>
                <dt style={{ color: 'var(--color-text-muted)' }}>Workflow ID</dt>
                <dd style={{ margin: 0, fontFamily: 'monospace' }}>{data.id}</dd>
                <dt style={{ color: 'var(--color-text-muted)' }}>Trip ID</dt>
                <dd style={{ margin: 0, fontFamily: 'monospace' }}>{data.tripId}</dd>
                <dt style={{ color: 'var(--color-text-muted)' }}>Current step</dt>
                <dd style={{ margin: 0 }}>{data.currentStep ?? '—'}</dd>
                <dt style={{ color: 'var(--color-text-muted)' }}>Status</dt>
                <dd style={{ margin: 0 }}>{humanizeStatus(data.status)}</dd>
                <dt style={{ color: 'var(--color-text-muted)' }}>Created</dt>
                <dd style={{ margin: 0 }}>{formatDateTime(data.createdAt)}</dd>
                <dt style={{ color: 'var(--color-text-muted)' }}>Completed</dt>
                <dd style={{ margin: 0 }}>{formatDateTime(data.completedAt)}</dd>
              </dl>
            </div>
          </section>
        </div>
      )}

      <ApprovalActionDialog
        open={dialogAction !== null}
        action={dialogAction ?? 'APPROVE'}
        onClose={() => setDialogAction(null)}
        onConfirm={handleConfirm}
        submitting={submitting}
      />
    </>
  )
}