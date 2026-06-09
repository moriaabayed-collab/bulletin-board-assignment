import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { UserStore } from '../store/user.store';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const userStore = inject(UserStore);
  const token = userStore.token();

  const authReq = token
    ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : req;

  return next(authReq).pipe(
    catchError(err => {
      if (err instanceof HttpErrorResponse && err.status === 401) {
        userStore.logout();
      }
      return throwError(() => err);
    }),
  );
};
