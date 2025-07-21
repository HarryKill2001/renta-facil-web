import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export class CustomValidators {
  static colombianPhone(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      
      const phoneRegex = /^(\+?57\s?)?[39]\d{2}\s?\d{3}\s?\d{4}$/;
      const cleanPhone = control.value.replace(/\s/g, '');
      
      if (!phoneRegex.test(cleanPhone)) {
        return { colombianPhone: true };
      }
      
      return null;
    };
  }

  static colombianDocument(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      
      const docRegex = /^\d{6,12}$/;
      
      if (!docRegex.test(control.value)) {
        return { colombianDocument: true };
      }
      
      return null;
    };
  }

  static minimumAge(minAge: number): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      
      const birthDate = new Date(control.value);
      const today = new Date();
      const age = today.getFullYear() - birthDate.getFullYear();
      const monthDiff = today.getMonth() - birthDate.getMonth();
      
      let actualAge = age;
      if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
        actualAge--;
      }
      
      if (actualAge < minAge) {
        return { minimumAge: { requiredAge: minAge, actualAge } };
      }
      
      return null;
    };
  }

  static futureDate(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      
      const selectedDate = new Date(control.value);
      const today = new Date();
      today.setHours(0, 0, 0, 0);
      
      if (selectedDate < today) {
        return { futureDate: true };
      }
      
      return null;
    };
  }

  static dateRange(startDateField: string, endDateField: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.parent) {
        return null;
      }
      
      const startDate = control.parent.get(startDateField)?.value;
      const endDate = control.parent.get(endDateField)?.value;
      
      if (!startDate || !endDate) {
        return null;
      }
      
      const start = new Date(startDate);
      const end = new Date(endDate);
      
      if (end <= start) {
        return { dateRange: true };
      }
      
      return null;
    };
  }

  static strongPassword(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      
      const password = control.value;
      const errors: any = {};
      
      if (password.length < 8) {
        errors.minLength = true;
      }
      
      if (!/[A-Z]/.test(password)) {
        errors.uppercase = true;
      }
      
      if (!/[a-z]/.test(password)) {
        errors.lowercase = true;
      }
      
      if (!/\d/.test(password)) {
        errors.number = true;
      }
      
      if (!/[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(password)) {
        errors.specialChar = true;
      }
      
      return Object.keys(errors).length > 0 ? { strongPassword: errors } : null;
    };
  }

  static confirmPassword(passwordField: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.parent) {
        return null;
      }
      
      const password = control.parent.get(passwordField)?.value;
      const confirmPassword = control.value;
      
      if (!password || !confirmPassword) {
        return null;
      }
      
      if (password !== confirmPassword) {
        return { confirmPassword: true };
      }
      
      return null;
    };
  }

  static vehicleModel(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      
      const model = control.value.trim();
      
      if (model.length < 2) {
        return { vehicleModel: { minLength: true } };
      }
      
      if (model.length > 50) {
        return { vehicleModel: { maxLength: true } };
      }
      
      // Allow letters, numbers, spaces, and common car model characters
      if (!/^[a-zA-Z0-9\s\-\.]+$/.test(model)) {
        return { vehicleModel: { invalidCharacters: true } };
      }
      
      return null;
    };
  }

  static vehicleYear(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      
      const year = parseInt(control.value, 10);
      const currentYear = new Date().getFullYear();
      const minYear = 1900;
      
      if (isNaN(year)) {
        return { vehicleYear: { invalid: true } };
      }
      
      if (year < minYear) {
        return { vehicleYear: { tooOld: true, minYear } };
      }
      
      if (year > currentYear + 1) {
        return { vehicleYear: { tooNew: true, maxYear: currentYear + 1 } };
      }
      
      return null;
    };
  }

  static positiveNumber(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      
      const value = parseFloat(control.value);
      
      if (isNaN(value)) {
        return { positiveNumber: { invalid: true } };
      }
      
      if (value <= 0) {
        return { positiveNumber: { notPositive: true } };
      }
      
      return null;
    };
  }

  static priceRange(min: number, max: number): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }
      
      const value = parseFloat(control.value);
      
      if (isNaN(value)) {
        return { priceRange: { invalid: true } };
      }
      
      if (value < min) {
        return { priceRange: { tooLow: true, min } };
      }
      
      if (value > max) {
        return { priceRange: { tooHigh: true, max } };
      }
      
      return null;
    };
  }
}