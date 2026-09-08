import { Spin } from 'antd'

export default function LoadingState({ message = 'Loading...' }: { message?: string }) {
  return (
    <div className="flex min-h-48 items-center justify-center">
      <Spin size="large" tip={message} />
    </div>
  )
}
