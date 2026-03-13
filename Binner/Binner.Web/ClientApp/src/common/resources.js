import _ from "underscore";
import { ImageTypes } from "../common/Types";

/**
 * Get an image url for the FullPage (datasheet cover) type image
 * @param {object} parentResource The parent resource containing the resourceSourceUrl and resourcePath
 * @param {array} imageMetadatas the imageMetadata array containing the images available for the resource
 * @param {function} predicate the optional predicate to use
 * @returns the img src url
 */
 export const getFullPageImage = (parentResource, imageMetadatas, predicate = null) => {
	return getResourceImage(parentResource, imageMetadatas, ImageTypes.FullPage, predicate);
};

/**
 * Get an image url for the ProductShot type image
 * @param {object} parentResource The parent resource containing the resourceSourceUrl and resourcePath
 * @param {array} imageMetadatas the imageMetadata array containing the images available for the resource
 * @param {ImageTypes} imageType the image type to fetch
 * @param {function} predicate the optional predicate to use
 * @returns the img src url
 */
export const getDatasheetImage = (parentResource, imageMetadatas, imageType, predicate = null) => {
	return getResourceImage(parentResource, imageMetadatas, imageType, predicate);
};

/**
 * Get an image url for the ProductShot type image
 * @param {array} imageMetadatas the imageMetadata array containing the images available for the resource
 * @param {function} predicate the optional predicate to use
 * @returns the img src url
 */
 export const getProductShotImage = (imageMetadatas, predicate = null) => {
	return getResourceImage(null, imageMetadatas, ImageTypes.ProductShot, predicate);
};

/**
 * Get an image url
 * @param {object} parentResource The optional parent resource containing the resourceSourceUrl and resourcePath
 * @param {array} imageMetadatas the imageMetadata array containing the images available for the resource
 * @param {ImageTypes} imageType the image type to get
 * @param {function} predicate the optional predicate to use
 * @returns the img src url
 */
 export const getResourceImage = (parentResource, imageMetadatas, imageType = ImageTypes.ProductShot, predicate = (s) => s.imageType === imageType) => {
	if (!imageMetadatas) return "";
	// default sort by isDefault
	const sortedImageMetadatas = imageMetadatas.sort((a,b) => {
		return Number(b.isDefault) - Number(a.isDefault);
	});
	const image = _.find(sortedImageMetadatas, predicate);
	if (!image) return "";
	let url = "";
	if (parentResource) {
		url = getResourceImageUrl(parentResource.resourceSourceUrl, parentResource.resourcePath, image.imageId);
	} else {
		url = getResourceImageUrl(image.resourceSourceUrl, image.resourcePath, image.imageId);
	}
	return url;
};

/**
 * Get an image url
 * @param {string} resourceUrl The resource url without method
 * @param {string} resourcePath resource folder path
 * @param {number} imageId the image type to get
 * @returns the img src url
 */
export const getResourceImageUrl = (resourceUrl, resourcePath, imageId = null, extension = ".png") => {
  if (resourceUrl?.length > 0 && resourcePath?.length > 0) {
	  if (imageId)
		  return `https://${resourceUrl}/${resourcePath}_${imageId}${extension}`;
    return `https://${resourceUrl}/${resourcePath}${extension}`;
  }
  return "";
};

/**
 * Get an image url from a resource object (new method, use this going forward)
 */
export const getUrlForResource = (resource) => {
  if (resource?.resourceSourceUrl?.length > 0 && resource?.resourcePath?.length > 0)
    return `https://${resource.resourceSourceUrl}/${resource.resourcePath}${resource.extension || '.png'}`;
  return "";
};

/**
 * Get a datasheet url
 * @param {string} resourceUrl The resource url without method
 * @param {string} resourcePath resource folder path
 * @returns the img src url
 */
export const getResourceDatasheetUrl = (resourceUrl, resourcePath, extension = ".pdf") => {
  if (resourceUrl?.length > 0 && resourcePath?.length > 0) {
	  return `https://${resourceUrl}/${resourcePath}${extension}`;
  }
  return "";
};

/**
 * Get the default ProductShot type image
 * @param {object} partNumber The part number
 * @param {string} className The custom classes to provide
 * @param {function} predicate the optional predicate to use
 * @returns The img tag
 */
export const getDefaultProductShotImageFromPartNumber = (partNumber, className = "product productshot large", predicate = null) => {
	if (!partNumber) return "";

	// if it has a default image set, use it
	if (partNumber.defaultImageId > 0) {
		const src = getResourceImageUrl(partNumber.defaultImageResourceSourceUrl, partNumber.defaultImageResourcePath, partNumber.defaultImageId);
		return (<img src={src} alt={partNumber.name} className={className} />)
	}

	// get the first image in the manufacturers image list
	const partNumberManufacturerWithImage = _.find(partNumber.partNumberManufacturers, i => i.imageMetadata && i.imageMetadata.length > 0);
	if (!partNumberManufacturerWithImage) return "";
	const src = getProductShotImage(partNumberManufacturerWithImage.imageMetadata, predicate);
	return (<img src={src} alt={partNumberManufacturerWithImage.name} className={className} />);
};

/**
 * Get the default ProductShot type image
 * @param {object} partNumberManufacturer The part number manufacturer
 * @param {string} className The custom classes to provide
 * @param {function} predicate the optional predicate to use
 * @returns The img tag
 */
 export const getDefaultProductShotImageFromPartNumberManufacturer = (partNumberManufacturer, className = "product productshot large", predicate = null) => {
	if (!partNumberManufacturer) return "";

	// if it has a default image set, use it
	if (partNumberManufacturer.defaultPartNumberManufacturerImageMetadataId > 0) {
		const defaultImage = partNumberManufacturer.imageMetadata.find(x => x.isDefault);
		if (defaultImage) {
			const src = getResourceImageUrl(defaultImage.resourceSourceUrl, defaultImage.resourcePath, defaultImage.imageId);
			return (<img src={src} alt={partNumberManufacturer.name} className={className} />);
		}
	}

	const src = getProductShotImage(partNumberManufacturer.imageMetadata, predicate);
	return (<img src={src} alt={partNumberManufacturer.name} className={className} />);
};