import { useMutation, UseMutationOptions, UseMutationResult, QueryKey, useQueryClient } from '@tanstack/react-query';
import { AxiosError } from 'axios';
import { useToast } from '../state/ToastContext';

type AnyFn = (...args: any[]) => Promise<any>;

interface ToastMutationOpts<TData, TError, TVariables, TContext> extends UseMutationOptions<TData, TError, TVariables, TContext> {
  successMessage?: string | ((data: TData) => string);
  errorMessage?: string | ((err: TError) => string);
  invalidateKeys?: QueryKey[];
}

export function useToastMutation<TData = unknown, TError = AxiosError<any>, TVariables = void, TContext = unknown>(
  mutationFn: (variables: TVariables) => Promise<TData>,
  opts?: ToastMutationOpts<TData, TError, TVariables, TContext>
): UseMutationResult<TData, TError, TVariables, TContext> {
  const { push } = useToast();
  const qc = useQueryClient();
  return useMutation<TData, TError, TVariables, TContext>({
    mutationFn,
    ...opts,
    onSuccess: (data, vars, ctx) => {
      if (opts?.successMessage) {
        const msg = typeof opts.successMessage === 'function' ? opts.successMessage(data) : opts.successMessage;
        push(msg, 'success');
      }
      if (opts?.invalidateKeys) {
        opts.invalidateKeys.forEach(k => qc.invalidateQueries({ queryKey: k }));
      }
      opts?.onSuccess?.(data, vars, ctx);
    },
    onError: (err, vars, ctx) => {
      if (opts?.errorMessage) {
        const msg = typeof opts.errorMessage === 'function' ? opts.errorMessage(err) : opts.errorMessage;
        push(msg, 'error');
      } else {
        const axiosErr = err as any as AxiosError<any>;
        const apiMsg = axiosErr?.response?.data?.title || axiosErr?.response?.data?.message;
        push(apiMsg || 'Operation failed', 'error');
      }
      opts?.onError?.(err, vars, ctx);
    }
  });
}
