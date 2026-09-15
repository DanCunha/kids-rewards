export default function ErrorMessage({ message }: { message: string }) {
  return (
    <div
      className="rounded-xl border border-rose-200 bg-rose-50 px-4 py-3 text-sm font-medium text-rose-700"
      role="alert"
    >
      {message}
    </div>
  );
}