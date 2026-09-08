import { Form, Input, Select } from 'antd'
import type {
  ChangeEvent,
  InputHTMLAttributes,
  ReactNode,
  TextareaHTMLAttributes,
} from 'react'
import { Children, isValidElement } from 'react'
import Button from './Button'

interface FormFieldProps {
  label: string
  hint?: string
  required?: boolean
  children: ReactNode
}

export function FormField({ label, hint, required, children }: FormFieldProps) {
  return (
    <Form.Item
      label={label}
      required={required}
      extra={hint}
      layout="vertical"
      className="mb-0"
    >
      {children}
    </Form.Item>
  )
}

export function TextInput({ size: _size, ...props }: InputHTMLAttributes<HTMLInputElement>) {
  return <Input {...props} />
}

interface SelectInputProps {
  children?: ReactNode
  className?: string
  defaultValue?: string | number
  disabled?: boolean
  onChange?: (event: ChangeEvent<HTMLSelectElement>) => void
  required?: boolean
  value?: string | number
}

export function SelectInput({
  children,
  className,
  defaultValue,
  disabled,
  onChange,
  required: _required,
  value,
}: SelectInputProps) {
  const options: Array<{ value: string | number; label: ReactNode; disabled?: boolean }> = Children.toArray(children).flatMap((child) => {
    if (!isValidElement<{ value?: string | number; disabled?: boolean; children?: ReactNode }>(child)) {
      return []
    }

    return [{
      value: child.props.value ?? '',
      label: child.props.children,
      disabled: child.props.disabled,
    }]
  })
  const placeholderOption = options.find((option) => option.value === '')
  const selectedValue = typeof value === 'string' || typeof value === 'number' ? value || undefined : undefined
  const initialValue = typeof defaultValue === 'string' || typeof defaultValue === 'number'
    ? defaultValue || undefined
    : undefined

  const notifyChange = (nextValue: string | number | undefined) => {
    const next = nextValue === undefined ? '' : String(nextValue)
    onChange?.({ target: { value: next }, currentTarget: { value: next } } as ChangeEvent<HTMLSelectElement>)
  }

  return (
    <Select
      className={className}
      disabled={disabled}
      value={selectedValue}
      defaultValue={initialValue}
      options={options.filter((option) => option.value !== '')}
      placeholder={placeholderOption?.label}
      allowClear={Boolean(placeholderOption)}
      showSearch={false}
      onChange={notifyChange}
    />
  )
}

export function TextArea(props: TextareaHTMLAttributes<HTMLTextAreaElement>) {
  return <Input.TextArea {...props} />
}

export function ModalFooterButtons({
  onCancel,
  submitLabel,
  loading,
  submitForm,
}: {
  onCancel: () => void
  submitLabel: string
  loading?: boolean
  submitForm?: string
}) {
  return (
    <>
      <Button variant="modal-primary" type="submit" form={submitForm} loading={loading}>
        {submitLabel}
      </Button>
      <Button variant="ghost" onClick={onCancel} disabled={loading}>
        Hủy
      </Button>
    </>
  )
}
