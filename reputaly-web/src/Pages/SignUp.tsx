import { SignUp } from '@clerk/clerk-react';
import styles from './Auth.module.css';

export default function SignUpPage() {
    return (
        <div className={styles.wrapper}>
            <SignUp routing="path" path="/sign-up" signInUrl="/sign-in" />
        </div>
    );
}