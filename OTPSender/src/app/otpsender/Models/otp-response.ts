export interface OtpResponse {
  success: boolean;
  message?: string;
  maxAttemptsReached?: boolean;
  expired?: boolean;
  attemptsLeft?: number;
  authenticationURL?: string;
  blocked?: boolean;
  blockExpiry?: string;
}
