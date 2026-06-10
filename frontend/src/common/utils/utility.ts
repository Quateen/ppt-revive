  export const getUserInitials = (fullName: string | undefined | null): string => {
    if (!fullName) return '';
    const names = fullName.trim().split(' ').filter(Boolean);

    if (names.length === 1) return names[0][0].toUpperCase();

    return names.slice(0, 2).map(n => n[0]?.toUpperCase()).join('');
  };