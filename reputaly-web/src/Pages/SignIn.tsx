import { SignIn } from '@clerk/clerk-react';
import styles from './Auth.module.css';

export default function SignInPage() {
    return (
        <div className={styles.wrapper}>
            <SignIn routing="path" path="/sign-in" signUpUrl="/sign-up" />
        </div>
    );
}