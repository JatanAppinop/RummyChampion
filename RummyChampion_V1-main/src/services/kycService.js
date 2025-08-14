const kycRepository = require('../repositories/kycRepository');
const userRepository = require('../repositories/userRepository')

class kycService {

    async submitKyc(userId, files, data) {
        try {
            let documentImage = "";
            if (files.documentImage && files.documentImage[0].path !== undefined) {
                documentImage = files.documentImage[0].path;
            }
            // const filePath = `uploads/${files.documentImage[0].filename}`;

            const kycData = {
                userId: userId,
                fullName: data.fullName,
                dob: data.dob,
                documentImage: documentImage,
                documentType: data.documentType,
                documentNumber: data.documentNumber
            };

            // Check if the user has already submitted KYC
            const existingKycRecord = await kycRepository.findByUserId(userId);
            if (existingKycRecord) {
                throw new Error("KYC already submitted for this user");
            }

            // Check if the document number already exists
            const existingDocumentRecord = await kycRepository.findByDocumentNumber(data.documentNumber);
            if (existingDocumentRecord) {
                throw new Error("Document number already exists");
            }

            // Create KYC record
            const kycRecord = await kycRepository.createKyc(kycData);

            // Update user KYC status
            await kycRepository.updateUserKycStatus(userId);

            return { kycRecord };
        } catch (error) {
            throw new Error(error.message);
        }
    }

    async getKycStatus(userId) {
        try {
            const user = await kycRepository.findUserById(userId);
            if (!user) {
                throw new Error("User not found");
            }

            let status;
            console.log(user, ">>>>>>>>");

            switch (user.kycVerified) {
                case 1:
                    status = "Pending";
                    break;
                case 2:
                    status = "Approved";
                    break;
                case 3:
                    status = "Rejected";
                    break;
                default:
                    status = "Not Applied";
            }

            return { status };
        } catch (error) {
            throw new Error(error.message);
        }
    }

    async getAllKyc() {
        try {
            const kycAll = await kycRepository.findKyc();
            return kycAll;
        } catch (error) {
            console.error("Error signing up:", error);
        }
    }

    async updateKyc(kycVerified, userId, reason) {
        try {
            const result = await kycRepository.updateKycStatus(kycVerified, userId, reason);
            return {
                success: true,
                message: `KYC status updated to ${kycVerified}`,
                data: result,
            };
        } catch (error) {
            throw new Error(`Error updating KYC status: ${error.message}`);
        }

    }

    async getBanks() {
        try {
            const banks = await kycRepository.findBanks();
            return banks;
        } catch (error) {
            console.error("error finding banks")
        }
    }

    async approveBank(userId, verified) {
        try {
            const bankApprove = await kycRepository.updateBanks(userId, verified)
            return bankApprove
        } catch (error) {
            console.error("error updating status")
        }
    }

    async getKycByStatus(status) {
        try {
            const kyc = await kycRepository.findKycByStatus(status);
            return kyc;
        } catch (error) {
            console.error("Error fetching KYC by status:", error);
            throw error;
        }
    }

    async getBankByStatus(status) {
        try {
            const banks = await kycRepository.findBankByStatus(status);
            return banks;
        } catch (error) {
            console.error("Error fetching banks by status:", error);
            throw error;
        }
    }

    async getUserKyc(userId) {
        const kyc = await kycRepository.findByUserId(userId);
        const user = await userRepository.findById(userId);
        return { kyc, user }
    }

    async getFundAccount(userId) {
        return await kycRepository.getFunds(userId);
    }
}

module.exports = new kycService()