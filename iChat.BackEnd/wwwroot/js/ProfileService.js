class ProfileService {
    static async getUserProfile() {
        let profile = StorageHelper.getItem("userProfile");

        if (!profile) {
            try {
                const response = await fetch("/api/user/profile");
                if (!response.ok) throw new Error("Failed to fetch profile data");

                profile = await response.json();
                StorageHelper.setItem("userProfile", profile, 10); 
            } catch (error) {
                console.error("Error fetching user profile:", error);
                return null;
            }
        }
        return profile;
    }
}
